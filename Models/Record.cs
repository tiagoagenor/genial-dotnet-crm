using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace genial_dotnet_crm.Models;

public class Record
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("collectionId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string CollectionId { get; set; } = string.Empty;

    [BsonElement("data")]
    public BsonDocument Data { get; set; } = new BsonDocument();

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}




