using FastTechFoods.Observability;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Api.DI
{
    public static class StartUpConfig
    {
        public static void AddObservability(WebApplicationBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE")
                ?? "mongodb://localhost:27017";

            builder.Services.AddSingleton<IMongoClient>(sp =>
            {
                var settings = MongoClientSettings.FromConnectionString(connectionString);
                
                // Set the ServerApi field to ensure consistent API version usage
                settings.ServerApi = new ServerApi(ServerApiVersion.V1);
                
                return new MongoClient(settings);
            });
            builder.Services.AddFastTechFoodsObservabilityWithSerilog(builder.Configuration);
            builder.Services.AddFastTechFoodsPrometheus(builder.Configuration);
            builder.Services.AddFastTechFoodsHealthChecksWithMongoDB(builder.Configuration, connectionString);
        }

        public static void UseObservability(WebApplication app)
        {
            app.UseFastTechFoodsHealthChecksUI();
            app.UseFastTechFoodsPrometheus();
        }
    }
}
