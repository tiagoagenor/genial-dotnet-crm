using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace genial_dotnet_crm.Models;

public class Collection
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;

    [BsonElement("name")]
    [BsonRequired]
    public string Name { get; set; } = string.Empty;

    [BsonElement("label")]
    public string Label { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("fields")]
    public List<CollectionField> Fields { get; set; } = new();

    [BsonElement("userId")]
    [BsonRepresentation(BsonType.ObjectId)]
    public string UserId { get; set; } = string.Empty;

    [BsonElement("stage")]
    public string Stage { get; set; } = "hml";

    [BsonElement("createdAt")]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [BsonElement("updatedAt")]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

public class CollectionField
{
    [BsonElement("name")]
    public string Name { get; set; } = string.Empty;

    [BsonElement("type")]
    public string Type { get; set; } = string.Empty;

    [BsonElement("label")]
    public string Label { get; set; } = string.Empty;

    [BsonElement("order")]
    public int Order { get; set; }

    [BsonElement("configuration")]
    [BsonIgnoreIfNull]
    public FieldConfiguration? Configuration { get; set; }
}

public class FieldConfiguration
{
    [BsonElement("minLength")]
    [BsonIgnoreIfNull]
    public string? MinLength { get; set; }

    [BsonElement("maxLength")]
    [BsonIgnoreIfNull]
    public string? MaxLength { get; set; }

    [BsonElement("validationPattern")]
    [BsonIgnoreIfNull]
    public string? ValidationPattern { get; set; }

    [BsonElement("nonempty")]
    public bool Nonempty { get; set; } = true;

    [BsonElement("hidden")]
    public bool Hidden { get; set; } = false;

    [BsonElement("options")]
    [BsonIgnoreIfNull]
    public List<SelectOption>? Options { get; set; }
}

public class SelectOption
{
    [BsonElement("key")]
    public string Key { get; set; } = string.Empty;

    [BsonElement("value")]
    public string Value { get; set; } = string.Empty;
}

public enum FieldTypeEnum
{
    PlainText,
    RichEditor,
    Number,
    Bool,
    Email,
    Url,
    DateTime,
    Autodate,
    Json,
    Select,
    File,
    Relation,
    GeoPoint,
    System
}

public static class FieldTypeEnumExtensions
{
    public static string ToStringValue(this FieldTypeEnum fieldType)
    {
        return fieldType switch
        {
            FieldTypeEnum.PlainText => "plain-text",
            FieldTypeEnum.RichEditor => "rich-editor",
            FieldTypeEnum.Number => "number",
            FieldTypeEnum.Bool => "bool",
            FieldTypeEnum.Email => "email",
            FieldTypeEnum.Url => "url",
            FieldTypeEnum.DateTime => "datetime",
            FieldTypeEnum.Autodate => "autodate",
            FieldTypeEnum.Json => "json",
            FieldTypeEnum.Select => "select",
            FieldTypeEnum.File => "file",
            FieldTypeEnum.Relation => "relation",
            FieldTypeEnum.GeoPoint => "geo-point",
            FieldTypeEnum.System => "system",
            _ => "plain-text"
        };
    }

    public static FieldTypeEnum FromString(string? value)
    {
        if (string.IsNullOrEmpty(value))
            return FieldTypeEnum.PlainText;

        return value.ToLower() switch
        {
            "plain-text" => FieldTypeEnum.PlainText,
            "rich-editor" => FieldTypeEnum.RichEditor,
            "number" => FieldTypeEnum.Number,
            "bool" => FieldTypeEnum.Bool,
            "email" => FieldTypeEnum.Email,
            "url" => FieldTypeEnum.Url,
            "datetime" => FieldTypeEnum.DateTime,
            "autodate" => FieldTypeEnum.Autodate,
            "json" => FieldTypeEnum.Json,
            "select" => FieldTypeEnum.Select,
            "file" => FieldTypeEnum.File,
            "relation" => FieldTypeEnum.Relation,
            "geo-point" => FieldTypeEnum.GeoPoint,
            "system" => FieldTypeEnum.System,
            _ => FieldTypeEnum.PlainText
        };
    }
}


