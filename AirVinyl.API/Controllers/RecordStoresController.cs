using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Results;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class RecordStoresController : ODataController
    {
        private AirVinylDbContext dbContext = new AirVinylDbContext();

        [EnableQuery]
        public IHttpActionResult Get()
        {
            return Ok(dbContext.RecordStores);
        }

        [EnableQuery]
        public IHttpActionResult Get([FromODataUri] int key)
        {
            IQueryable<RecordStore> recordsStore = dbContext.RecordStores.Where(r => r.RecordStoreId == key);

            if (!recordsStore.Any())
            {
                return NotFound();
            }

            return Ok(SingleResult.Create(recordsStore));
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("RecordStores({recordStoreId})/Tags")]
        public IHttpActionResult GetTags([FromODataUri] int recordStoreId)
        {
            RecordStore recordStore = dbContext.RecordStores.FirstOrDefault(r => r.RecordStoreId == recordStoreId);

            if (recordStore == null)
            {
                return NotFound();
            }

            return Ok(recordStore.Tags);
        }

        [HttpGet]
        [ODataRoute("RecordStores({recordStoreId})/AirVinyl.Functions.IsHighRated(minimumRating={minimumRating})")]
        public bool IsHighRated(int recordStoreId, int minimumRating)
        {
            RecordStore recordStore = dbContext.RecordStores.FirstOrDefault(r => r.RecordStoreId == recordStoreId
                && r.Ratings.Any()
                && r.Ratings.Sum(rt => rt.Value) / r.Ratings.Count() >= minimumRating);

            return recordStore != null;
        }

        [HttpGet]
        [ODataRoute("RecordStores/AirVinyl.Functions.AreRatedBy(personIds={personIds})")]
        public IHttpActionResult AreRatedBy([FromODataUri] IEnumerable<int> personIds)
        {
            IQueryable<RecordStore> recordStores = dbContext.RecordStores
                .Where(r => r.Ratings.Any(rt => personIds.Contains(rt.RatedBy.PersonId)));

            return this.CreateOKHttpActionResult(recordStores);
        }

        [HttpGet]
        [ODataRoute("GetHighRatedRecordStores(minimumRating={minimumRating})")]
        public IHttpActionResult GetHighRatedRecordStores(int minimumRating)
        {
            IQueryable<RecordStore> recordStores = dbContext.RecordStores
                .Where(r => r.Ratings.Any()
                && r.Ratings.Sum(rt => rt.Value) / r.Ratings.Count() >= minimumRating);

            return this.CreateOKHttpActionResult(recordStores);
        }

        [HttpPost]
        [ODataRoute("RecordStores({recordStoreId})/AirVinyl.Actions.Rate")]
        public IHttpActionResult Rate(int recordStoreId, ODataActionParameters parameters)
        {
            RecordStore recordStore = dbContext.RecordStores.FirstOrDefault(r => r.RecordStoreId == recordStoreId);

            if (recordStore == null)
            {
                return NotFound();
            }

            object value;
            int rating;
            int personId;

            if (!parameters.TryGetValue("rating", out value) || !int.TryParse(value.ToString(), out rating)
                || !parameters.TryGetValue("personId", out value) || !int.TryParse(value.ToString(), out personId))
            {
                return BadRequest();
            }

            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            recordStore.Ratings.Add(new Rating { RatedBy = person, Value = rating });

            return this.CreateOKHttpActionResult(dbContext.SaveChanges() >= 0);
        }

        [HttpPost]
        [ODataRoute("RecordStores/AirVinyl.Actions.RemoveRatings")]
        public IHttpActionResult RemoveRatings(ODataActionParameters parameters)
        {
            object value;
            int personId;

            if (!parameters.TryGetValue("personId", out value) || !int.TryParse(value.ToString(), out personId))
            {
                return BadRequest();
            }

            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == personId);

            if (person == null)
            {
                return NotFound();
            }

            foreach (var recordStore in dbContext.RecordStores.Include("Ratings").Include("Ratings.RatedBy"))
            {
                List<Rating> ratings = recordStore.Ratings.Where(rt => rt.RatedBy == person).ToList();
                foreach (var rating in ratings)
                {
                    recordStore.Ratings.Remove(rating);
                }
            }

            return this.CreateOKHttpActionResult(dbContext.SaveChanges() >= 0);
        }

        [HttpPost]
        [ODataRoute("RemoveRecordStoreRatings")]
        public IHttpActionResult RemoveRecordStoreRatings(ODataActionParameters parameters)
        {
            return ((OkNegotiatedContentResult<bool>)RemoveRatings(parameters)).Content
                ? StatusCode(HttpStatusCode.NoContent)
                : StatusCode(HttpStatusCode.InternalServerError);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}