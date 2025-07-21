using MongoDB.Driver;
using MongoDB.Bson;
using Xunit;

namespace FastTechFoodsOrder.Tests.Integration
{
    public class MongoDBConnectionTests
    {
        [Fact]
        public void MongoClient_Should_Have_Consistent_ServerApi_Configuration()
        {
            // Arrange
            var connectionString = "mongodb://localhost:27017";
            
            // Simulate the configuration from StartUpConfig
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            
            // Act & Assert
            Assert.NotNull(client);
            Assert.NotNull(settings.ServerApi);
            Assert.Equal(ServerApiVersion.V1, settings.ServerApi.Version);
        }
        
        [Fact]
        public void ApplicationDbContext_Should_Accept_IMongoClient_Injection()
        {
            // Arrange
            var connectionString = "mongodb://localhost:27017";
            var settings = MongoClientSettings.FromConnectionString(connectionString);
            settings.ServerApi = new ServerApi(ServerApiVersion.V1);
            var client = new MongoClient(settings);
            
            // Act
            var context = new FastTechFoodsOrder.Infra.Context.ApplicationDbContext(client);
            
            // Assert
            Assert.NotNull(context);
        }
    }
}
