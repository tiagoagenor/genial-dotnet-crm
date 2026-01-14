using MongoDB.Driver;
using genial_dotnet_crm.Data;
using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public class CollectionService : ICollectionService
{
    private readonly IMongoCollection<Collection> _collections;

    public CollectionService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _collections = database.GetCollection<Collection>("collections");
    }

    public async Task<Collection> CreateCollectionAsync(Collection collection)
    {
        collection.CreatedAt = DateTime.UtcNow;
        collection.UpdatedAt = DateTime.UtcNow;
        await _collections.InsertOneAsync(collection);
        return collection;
    }

    public async Task<Collection> UpdateCollectionAsync(string id, Collection collection)
    {
        collection.UpdatedAt = DateTime.UtcNow;
        await _collections.ReplaceOneAsync(c => c.Id == id, collection);
        return collection;
    }

    public async Task<List<Collection>> GetCollectionsByUserAsync(string userId, string stage)
    {
        var filter = Builders<Collection>.Filter.And(
            Builders<Collection>.Filter.Eq(c => c.UserId, userId),
            Builders<Collection>.Filter.Eq(c => c.Stage, stage)
        );
        return await _collections.Find(filter).ToListAsync();
    }

    public async Task<Collection?> GetCollectionByIdAsync(string id)
    {
        return await _collections.Find(c => c.Id == id).FirstOrDefaultAsync();
    }

    public async Task<Collection?> GetCollectionByNameAsync(string name, string userId, string stage)
    {
        var filter = Builders<Collection>.Filter.And(
            Builders<Collection>.Filter.Eq(c => c.Name, name),
            Builders<Collection>.Filter.Eq(c => c.UserId, userId),
            Builders<Collection>.Filter.Eq(c => c.Stage, stage)
        );
        return await _collections.Find(filter).FirstOrDefaultAsync();
    }

    public async Task<bool> DeleteCollectionAsync(string id)
    {
        var result = await _collections.DeleteOneAsync(c => c.Id == id);
        return result.DeletedCount > 0;
    }
}

