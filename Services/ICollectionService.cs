using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public interface ICollectionService
{
    Task<Collection> CreateCollectionAsync(Collection collection);
    Task<Collection> UpdateCollectionAsync(string id, Collection collection);
    Task<List<Collection>> GetCollectionsByUserAsync(string userId, string stage);
    Task<Collection?> GetCollectionByIdAsync(string id);
    Task<Collection?> GetCollectionByNameAsync(string name, string userId, string stage);
    Task<bool> DeleteCollectionAsync(string id);
}

