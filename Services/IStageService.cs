using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public interface IStageService
{
    Task<List<Stage>> GetAllStagesAsync();
    Task<Stage?> GetStageByKeyAsync(string key);
    Task SeedDefaultStagesAsync();
}


