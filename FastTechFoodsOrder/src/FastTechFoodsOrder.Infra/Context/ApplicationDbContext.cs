using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.Context
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoClient _client;

        public ApplicationDbContext(IConfiguration configuration)
        {
            string connectionUri = Environment.GetEnvironmentVariable("CONNECTION_STRING_DATABASE");

            var settings = MongoClientSettings.FromConnectionString(connectionUri);

            // Set the ServerApi field of the settings object to set the version of the Stable API on the client
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);

            // Create a new client and connect to the server
            var client = new MongoClient(settings);
            _database = client.GetDatabase("FastTechFoodsOrder");
            // Send a ping to confirm a successful connection
            try
            {
                var result = client.GetDatabase("FastTechFoodsOrder").RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Pinged your deployment. You successfully connected to MongoDB!");
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

        }

        public IMongoCollection<T> GetCollection<T>(string name) =>
            _database.GetCollection<T>(name);
    }
}
