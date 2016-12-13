using AirVinyl.Model;
using Microsoft.OData.Edm;
using System.Web.Http;
using System.Web.OData.Builder;
using System.Web.OData.Extensions;

namespace AirVinyl.API
{
    public static class WebApiConfig
    {
        public static void Register(HttpConfiguration config)
        {
            config.MapODataServiceRoute("ODataRoute", "odata", GetEdmModel());
            config.Count().Filter().OrderBy().Expand().Select().MaxTop(null);
            config.EnsureInitialized();
        }

        private static IEdmModel GetEdmModel()
        {
            var builder = new ODataConventionModelBuilder()
            {
                Namespace = "AirVinyl",
                ContainerName = "AirVinylContainer",
            };

            builder.EntitySet<Person>("People");
            builder.EntitySet<VinylRecord>("VinylRecords");
            builder.EntitySet<RecordStore>("RecordStores");

            return builder.GetEdmModel();
        }
    }
}
