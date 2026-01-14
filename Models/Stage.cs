using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace genial_dotnet_crm.Models;

public class Stage
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("key")]
    [BsonRequired]
    public string Key { get; set; } = string.Empty; // dev, hml, prod

    [BsonElement("label")]
    [BsonRequired]
    public string Label { get; set; } = string.Empty; // Dev, HML, Prod

    [BsonElement("letter")]
    public string Letter { get; set; } = string.Empty; // D, H, P

    [BsonElement("description")]
    public string Description { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("active")]
    public bool Active { get; set; } = true;
}


