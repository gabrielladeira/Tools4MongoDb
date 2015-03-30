using MongoDB.Bson;
using MongoDB.Driver;

namespace Tools4MongoDb
{
    public static class MongoHelper
    {
        public static MongoCollection<BsonDocument> GetCollection(string url, string databaseName, string collectionName)
        {
            var client = new MongoClient(url);
            var server = client.GetServer();
            var database = server.GetDatabase(databaseName);

            return database.GetCollection<BsonDocument>(collectionName);
        }
    }
}