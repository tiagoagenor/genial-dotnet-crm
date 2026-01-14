using genial_dotnet_crm.Models;
using MongoDB.Bson;

namespace genial_dotnet_crm.Services;

public interface IRecordService
{
    Task<BsonDocument> CreateRecordAsync(string collectionName, BsonDocument recordData, string userId, string stage);
    Task<bool> CollectionExistsAsync(string collectionName, string userId, string stage);
    Task<List<BsonDocument>> GetRecordsAsync(string collectionName);
}

