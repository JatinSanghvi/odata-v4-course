using AirVinyl.DataAccessLayer;
using AirVinyl.Model;
using System.Linq;
using System.Web.Http;
using System.Web.OData;
using System.Web.OData.Routing;

namespace AirVinyl.API.Controllers
{
    public class VinylRecordsController : ODataController
    {
        private AirVinylDbContext dbContext = new AirVinylDbContext();

        [HttpGet]
        [ODataRoute("VinylRecords")]
        public IHttpActionResult GetAllVinylRecords()
        {
            return Ok(dbContext.VinylRecords);
        }

        [HttpGet]
        [ODataRoute("VinylRecords({vinylRecordId})")]
        public IHttpActionResult GetOneVinylRecord([FromODataUri] int vinylRecordId)
        {
            VinylRecord vinylRecord = dbContext.VinylRecords.FirstOrDefault(v => v.VinylRecordId == vinylRecordId);

            if (vinylRecord == null)
            {
                return NotFound();
            }

            return Ok(vinylRecord);
        }

        protected override void Dispose(bool disposing)
        {
            dbContext.Dispose();
            base.Dispose(disposing);
        }
    }
}