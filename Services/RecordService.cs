using MongoDB.Driver;
using genial_dotnet_crm.Data;
using MongoDB.Bson;

namespace genial_dotnet_crm.Services;

public class RecordService : IRecordService
{
    private readonly IMongoDatabase _database;
    private readonly MongoDbSettings _settings;

    public RecordService(MongoDbSettings settings)
    {
        _settings = settings;
        var client = new MongoClient(settings.ConnectionString);
        _database = client.GetDatabase(settings.DatabaseName);
    }

    public async Task<bool> CollectionExistsAsync(string collectionName, string userId, string stage)
    {
        // Check if a collection with this name exists in the collections metadata
        var collectionsCollection = _database.GetCollection<BsonDocument>("collections");
        var filter = Builders<BsonDocument>.Filter.And(
            Builders<BsonDocument>.Filter.Eq("name", collectionName),
            Builders<BsonDocument>.Filter.Eq("userId", userId),
            Builders<BsonDocument>.Filter.Eq("stage", stage)
        );
        var collection = await collectionsCollection.Find(filter).FirstOrDefaultAsync();
        return collection != null;
    }

    public async Task<BsonDocument> CreateRecordAsync(string collectionName, BsonDocument recordData, string userId, string stage)
    {
        // Add system fields
        recordData["_id"] = ObjectId.GenerateNewId();
        recordData["created"] = DateTime.UtcNow;
        recordData["updated"] = DateTime.UtcNow;

        // Get or create the dynamic collection
        var dynamicCollection = _database.GetCollection<BsonDocument>(collectionName);
        
        // Insert the record
        await dynamicCollection.InsertOneAsync(recordData);

        return recordData;
    }

    public async Task<List<BsonDocument>> GetRecordsAsync(string collectionName)
    {
        try
        {
            var dynamicCollection = _database.GetCollection<BsonDocument>(collectionName);
            var records = await dynamicCollection.Find(_ => true).ToListAsync();
            return records;
        }
        catch
        {
            // Collection doesn't exist, return empty list
            return new List<BsonDocument>();
        }
    }
}

