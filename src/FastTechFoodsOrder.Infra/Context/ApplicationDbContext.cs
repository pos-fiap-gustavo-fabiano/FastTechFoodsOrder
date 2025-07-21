using Microsoft.Extensions.Configuration;
using MongoDB.Bson;
using MongoDB.Driver;

namespace FastTechFoodsOrder.Infra.Context
{
    public class ApplicationDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly IMongoClient _client;

        public ApplicationDbContext(IMongoClient client)
        {
            _client = client;
            _database = client.GetDatabase("FastTechFoodsOrder");
            
            // Send a ping to confirm a successful connection
            try
            {
                var result = _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
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
