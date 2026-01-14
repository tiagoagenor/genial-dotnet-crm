using MongoDB.Driver;
using genial_dotnet_crm.Data;
using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public class FieldTypeService : IFieldTypeService
{
    private readonly IMongoCollection<FieldType> _fieldTypes;

    public FieldTypeService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _fieldTypes = database.GetCollection<FieldType>("_fields");
    }

    public async Task<List<FieldType>> GetAllFieldTypesAsync()
    {
        var filter = Builders<FieldType>.Filter.Eq(f => f.Active, true);
        return await _fieldTypes.Find(filter).SortBy(f => f.Order).ToListAsync();
    }

    public async Task<FieldType?> GetFieldTypeByTypeAsync(string type)
    {
        return await _fieldTypes.Find(f => f.Type == type && f.Active).FirstOrDefaultAsync();
    }

    public async Task<FieldType> CreateFieldTypeAsync(FieldType fieldType)
    {
        fieldType.CreatedAt = DateTime.UtcNow;
        fieldType.UpdatedAt = DateTime.UtcNow;
        await _fieldTypes.InsertOneAsync(fieldType);
        return fieldType;
    }

    public async Task<FieldType> UpdateFieldTypeAsync(string id, FieldType fieldType)
    {
        fieldType.UpdatedAt = DateTime.UtcNow;
        await _fieldTypes.ReplaceOneAsync(f => f.Id == id, fieldType);
        return fieldType;
    }

    public async Task<bool> DeleteFieldTypeAsync(string id)
    {
        var result = await _fieldTypes.DeleteOneAsync(f => f.Id == id);
        return result.DeletedCount > 0;
    }

    public async Task SeedDefaultFieldTypesAsync()
    {
        // Verificar se já existem campos
        var existingCount = await _fieldTypes.CountDocumentsAsync(_ => true);
        if (existingCount > 0)
        {
            return; // Já existe dados, não precisa popular
        }

        var defaultFields = new List<FieldType>
        {
            new FieldType { Type = "plain-text", Label = "Plain text", Icon = "fas fa-font", DisplayIcon = "T", Order = 1 },
            new FieldType { Type = "rich-editor", Label = "Rich editor", Icon = "fas fa-pen", Order = 2 },
            new FieldType { Type = "number", Label = "Number", Icon = "fas fa-hashtag", DisplayIcon = "#", Order = 3 },
            new FieldType { Type = "bool", Label = "Bool", Icon = "fas fa-eye", Order = 4 },
            new FieldType { Type = "email", Label = "Email", Icon = "fas fa-envelope", Order = 5 },
            new FieldType { Type = "url", Label = "URL", Icon = "fas fa-link", Order = 6 },
            new FieldType { Type = "datetime", Label = "Datetime", Icon = "fas fa-calendar", Order = 7 },
            new FieldType { Type = "autodate", Label = "Autodate", Icon = "fas fa-calendar-check", Order = 8 },
            new FieldType { Type = "select", Label = "Select", Icon = "fas fa-list", Order = 9 },
            new FieldType { Type = "file", Label = "File", Icon = "fas fa-image", Order = 10 },
            new FieldType { Type = "relation", Label = "Relation", Icon = "fas fa-project-diagram", Order = 11 },
            new FieldType { Type = "json", Label = "JSON", Icon = "fas fa-code", DisplayIcon = "{}", Order = 12 },
            new FieldType { Type = "geo-point", Label = "Geo Point", Icon = "fas fa-map-marker-alt", Order = 13 }
        };

        await _fieldTypes.InsertManyAsync(defaultFields);
    }
}


