using AirVinyl.API.Helpers;
using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class SingletonController : ODataController
    {
        private AirVinylDbContext dbContext = new AirVinylDbContext();

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim")]
        public IHttpActionResult GetTim()
        {
            Person personTim = dbContext.People.FirstOrDefault(p => p.PersonId == 6);

            if (personTim == null)
            {
                return NotFound();
            }

            return Ok(personTim);
        }

        [HttpGet]
        [ODataRoute("Tim/Email")]
        [ODataRoute("Tim/FirstName")]
        [ODataRoute("Tim/LastName")]
        [ODataRoute("Tim/DateOfBirth")]
        [ODataRoute("Tim/Gender")]
        public IHttpActionResult GetPersonProperty()
        {
            Person personTim = dbContext.People.FirstOrDefault(p => p.PersonId == 6);

            if (personTim == null)
            {
                return NotFound();
            }

            string propertyName = Url.Request.RequestUri.Segments.Last();

            if (!personTim.HasProperty(propertyName))
            {
                return NotFound();
            }

            object propertyValue = personTim.GetValue(propertyName);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue);
        }

        [HttpGet]
        [ODataRoute("Tim/Email/$value")]
        [ODataRoute("Tim/FirstName/$value")]
        [ODataRoute("Tim/LastName/$value")]
        [ODataRoute("Tim/DateOfBirth/$value")]
        [ODataRoute("Tim/Gender/$value")]
        public IHttpActionResult GetPersonRawProperty()
        {
            Person personTim = dbContext.People.FirstOrDefault(p => p.PersonId == 6);

            if (personTim == null)
            {
                return NotFound();
            }

            string propertyName = Url.Request.RequestUri.Segments[Url.Request.RequestUri.Segments.Length - 2].TrimEnd('/');

            if (!personTim.HasProperty(propertyName))
            {
                return NotFound();
            }

            object propertyValue = personTim.GetValue(propertyName);

            if (propertyValue == null)
            {
                return StatusCode(HttpStatusCode.NoContent);
            }

            return this.CreateOKHttpActionResult(propertyValue.ToString());
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim/Friends")]
        public IHttpActionResult GetFriends()
        {
            Person person = dbContext.People.Include("Friends").FirstOrDefault(p => p.PersonId == 6);

            if (person == null || person.Friends == null)
            {
                return NotFound();
            }

            return Ok(person.Friends);
        }

        [HttpGet]
        [EnableQuery]
        [ODataRoute("Tim/VinylRecords")]
        public IHttpActionResult GetVinylRecords()
        {
            Person person = dbContext.People.FirstOrDefault(p => p.PersonId == 6);

            if (person == null)
            {
                return NotFound();
            }

            return Ok(dbContext.VinylRecords.Where(v => v.Person.PersonId == 6));
        }

        [HttpPatch]
        [ODataRoute("Tim")]
        public IHttpActionResult UpdateTim(Delta<Person> patch)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            Person personTim = dbContext.People.FirstOrDefault(p => p.PersonId == 6);

            if (personTim == null)
            {
                return NotFound();
            }

            patch.Patch(personTim);
            dbContext.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}