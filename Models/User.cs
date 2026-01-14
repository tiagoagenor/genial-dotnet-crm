using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace genial_dotnet_crm.Models;

public class User
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("email")]
    [BsonRequired]
    public string Email { get; set; } = string.Empty;

    [BsonElement("passwordHash")]
    [BsonRequired]
    public string PasswordHash { get; set; } = string.Empty;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}




