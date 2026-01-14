using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace genial_dotnet_crm.Models;

public class FieldType
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("type")]
    [BsonRequired]
    public string Type { get; set; } = string.Empty; // plain-text, rich-editor, number, etc.

    [BsonElement("label")]
    [BsonRequired]
    public string Label { get; set; } = string.Empty; // Plain text, Rich editor, etc.

    [BsonElement("icon")]
    [BsonRequired]
    public string Icon { get; set; } = string.Empty; // fas fa-font, fas fa-pen, etc.

    [BsonElement("displayIcon")]
    public string? DisplayIcon { get; set; } // Para ícones especiais como "T", "#", "{}"

    [BsonElement("description")]
    public string? Description { get; set; }

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; } = true;

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}


