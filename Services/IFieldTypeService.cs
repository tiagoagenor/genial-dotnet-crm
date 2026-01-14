using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public interface IFieldTypeService
{
    Task<List<FieldType>> GetAllFieldTypesAsync();
    Task<FieldType?> GetFieldTypeByTypeAsync(string type);
    Task<FieldType> CreateFieldTypeAsync(FieldType fieldType);
    Task<FieldType> UpdateFieldTypeAsync(string id, FieldType fieldType);
    Task<bool> DeleteFieldTypeAsync(string id);
    Task SeedDefaultFieldTypesAsync();
}


