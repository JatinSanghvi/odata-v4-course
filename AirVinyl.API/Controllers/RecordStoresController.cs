using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System.Linq;
using System.Web.Http;
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

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}