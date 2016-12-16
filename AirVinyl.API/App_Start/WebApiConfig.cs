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
            builder.EntitySet<RecordStore>("RecordStores");

            var isHighRatedFunction = builder.EntityType<RecordStore>().Function("IsHighRated");
            isHighRatedFunction.Namespace = "AirVinyl.Functions";
            isHighRatedFunction.Parameter<int>("minimumRating");
            isHighRatedFunction.Returns<bool>();

            var areRatedByFunction = builder.EntityType<RecordStore>().Collection.Function("AreRatedBy");
            areRatedByFunction.Namespace = "AirVinyl.Functions";
            areRatedByFunction.CollectionParameter<int>("personIds");
            areRatedByFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");

            var getHighRatedRecordStoresFunction = builder.Function("GetHighRatedRecordStores");
            getHighRatedRecordStoresFunction.Namespace = "AirVinyl.Functions";
            getHighRatedRecordStoresFunction.Parameter<int>("minimumRating");
            getHighRatedRecordStoresFunction.ReturnsCollectionFromEntitySet<RecordStore>("RecordStores");

            var rateAction = builder.EntityType<RecordStore>().Action("Rate");
            rateAction.Namespace = "AirVinyl.Actions";
            rateAction.Parameter<int>("rating");
            rateAction.Parameter<int>("personId");
            rateAction.Returns<bool>();

            var removeRatingsAction = builder.EntityType<RecordStore>().Collection.Action("RemoveRatings");
            removeRatingsAction.Namespace = "AirVinyl.Actions";
            removeRatingsAction.Parameter<int>("personId");
            removeRatingsAction.Returns<bool>();

            var removeRecordStoreRatingsAction = builder.Action("RemoveRecordStoreRatings");
            removeRecordStoreRatingsAction.Namespace = "AirVinyl.Actions";
            removeRecordStoreRatingsAction.Parameter<int>("personId");

            builder.Singleton<Person>("Tim");

            return builder.GetEdmModel();
        }
    }
}
