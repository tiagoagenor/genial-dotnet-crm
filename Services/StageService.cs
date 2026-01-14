using MongoDB.Driver;
using genial_dotnet_crm.Data;
using genial_dotnet_crm.Models;

namespace genial_dotnet_crm.Services;

public class StageService : IStageService
{
    private readonly IMongoCollection<Stage> _stages;

    public StageService(MongoDbSettings settings)
    {
        var client = new MongoClient(settings.ConnectionString);
        var database = client.GetDatabase(settings.DatabaseName);
        _stages = database.GetCollection<Stage>("_stage");
    }

    public async Task<List<Stage>> GetAllStagesAsync()
    {
        return await _stages.Find(s => s.Active).SortBy(s => s.Order).ToListAsync();
    }

    public async Task<Stage?> GetStageByKeyAsync(string key)
    {
        return await _stages.Find(s => s.Key == key && s.Active).FirstOrDefaultAsync();
    }

    public async Task SeedDefaultStagesAsync()
    {
        var count = await _stages.CountDocumentsAsync(_ => true);
        if (count > 0) return;

        var defaultStages = new List<Stage>
        {
            new Stage { Key = "dev", Label = "Dev", Letter = "D", Description = "Development environment", Order = 1 },
            new Stage { Key = "hml", Label = "HML", Letter = "H", Description = "Homologation environment", Order = 2 },
            new Stage { Key = "prod", Label = "Prod", Letter = "P", Description = "Production environment", Order = 3 }
        };

        await _stages.InsertManyAsync(defaultStages);
    }
}


