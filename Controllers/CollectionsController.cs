using Microsoft.AspNetCore.Mvc;
using genial_dotnet_crm.Models;
using genial_dotnet_crm.Services;
using genial_dotnet_crm.Data;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

namespace genial_dotnet_crm.Controllers;

public class CollectionsController : Controller
{
    private readonly ILogger<CollectionsController> _logger;
    private readonly ICollectionService _collectionService;
    private readonly IRecordService _recordService;
    private readonly IFieldTypeService _fieldTypeService;
    private readonly IStageService _stageService;
    private readonly IUserSessionService _userSessionService;

    public CollectionsController(
        ILogger<CollectionsController> logger,
        ICollectionService collectionService,
        IRecordService recordService,
        IFieldTypeService fieldTypeService,
        IStageService stageService,
        IUserSessionService userSessionService)
    {
        _logger = logger;
        _collectionService = collectionService;
        _recordService = recordService;
        _fieldTypeService = fieldTypeService;
        _stageService = stageService;
        _userSessionService = userSessionService;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = _userSessionService.GetUserId();
        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();

        var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);
        var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Collections"] = collections;
        ViewData["FieldTypes"] = fieldTypes;
        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;

        return View(collections);
    }

    [HttpGet]
    [Route("Collections/New")]
    public async Task<IActionResult> NewCollection()
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = _userSessionService.GetUserId();
        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();

        var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);
        var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Collections"] = collections;
        ViewData["FieldTypes"] = fieldTypes;
        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;
        ViewData["IsEditMode"] = false;

        return View(new Collection());
    }

    [HttpGet]
    [Route("Collections/{id}/Edit")]
    public async Task<IActionResult> Edit(string id)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = _userSessionService.GetUserId();
        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();

        var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);
        var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Collections"] = collections;
        ViewData["FieldTypes"] = fieldTypes;
        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;

        var model = await _collectionService.GetCollectionByIdAsync(id);
        if (model == null || model.UserId != userId)
        {
            TempData["ErrorMessage"] = "Collection not found";
            return RedirectToAction("Index");
        }

        ViewData["CollectionId"] = id;
        ViewData["IsEditMode"] = true;
        return View("NewCollection", model);
    }

    [HttpGet]
    [Route("Collections/FieldConfig/{type}")]
    public IActionResult GetFieldConfig(string type)
    {
        var viewPath = $"FieldConfigs/{type.ToLower()}";
        
        // Check if the view exists, if not use default
        // In a real scenario we'd use IViewEngine, but for now we'll rely on the switch or just direct names
        var viewName = type.ToLower() switch
        {
            "plain-text" => viewPath,
            "select" => viewPath,
            _ => "FieldConfigs/default"
        };

        return PartialView(viewName);
    }

    [HttpPost]
    [Route("Collections")]
    public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionRequest request)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Not authenticated" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var stage = _userSessionService.GetStage();

            // Validate duplicate system name
            var existingByName = await _collectionService.GetCollectionByNameAsync(request.Name, userId, stage);
            if (existingByName != null)
            {
                return Json(new { success = false, message = $"A collection with system name '{request.Name}' already exists." });
            }

            var collection = new Collection
            {
                Name = request.Name,
                Label = request.Label,
                Type = request.Type ?? "Base",
                Fields = request.Fields ?? new List<CollectionField>(),
                UserId = userId,
                Stage = stage
            };

            var created = await _collectionService.CreateCollectionAsync(collection);
            return Json(new { success = true, collection = created });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating collection");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpPut]
    [Route("Collections/{id}")]
    public async Task<IActionResult> UpdateCollection(string id, [FromBody] CreateCollectionRequest request)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Not authenticated" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var existing = await _collectionService.GetCollectionByIdAsync(id);

            if (existing == null || existing.UserId != userId)
            {
                return Json(new { success = false, message = "Collection not found" });
            }

            existing.Label = request.Label;
            existing.Type = request.Type ?? "Base";
            existing.Fields = request.Fields ?? new List<CollectionField>();

            // If name is being changed, check for conflict with other collections
            if (existing.Name != request.Name)
            {
                var conflict = await _collectionService.GetCollectionByNameAsync(request.Name, userId, _userSessionService.GetStage());
                if (conflict != null && conflict.Id != id)
                {
                    return Json(new { success = false, message = $"A collection with system name '{request.Name}' already exists." });
                }
                existing.Name = request.Name;
            }

            var updated = await _collectionService.UpdateCollectionAsync(id, existing);
            return Json(new { success = true, collection = updated });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating collection");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpDelete]
    [Route("Collections/{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Not authenticated" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var collection = await _collectionService.GetCollectionByIdAsync(id);

            if (collection == null || collection.UserId != userId)
            {
                return Json(new { success = false, message = "Collection not found" });
            }

            // Delete the collection metadata
            var deleted = await _collectionService.DeleteCollectionAsync(id);
            
            if (!deleted)
            {
                return Json(new { success = false, message = "Failed to delete collection" });
            }

            // Also delete the dynamic collection that stores records (if it exists)
            var stage = _userSessionService.GetStage();
            var collectionName = $"{stage}_{collection.Name}";
            try
            {
                var mongoDbSettings = HttpContext.RequestServices.GetRequiredService<MongoDbSettings>();
                var client = new MongoDB.Driver.MongoClient(mongoDbSettings.ConnectionString);
                var database = client.GetDatabase(mongoDbSettings.DatabaseName);
                await database.DropCollectionAsync(collectionName);
            }
            catch
            {
                // Collection might not exist, ignore error
            }

            _logger.LogInformation("Collection {CollectionId} deleted by user {UserId}", id, userId);
            return Json(new { success = true, message = "Collection deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting collection");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Collection(string id)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = _userSessionService.GetUserId();
        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();

        var collection = await _collectionService.GetCollectionByIdAsync(id);
        if (collection == null || collection.UserId != userId)
        {
            return RedirectToAction("Index");
        }

        var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);
        var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Collections"] = collections;
        ViewData["FieldTypes"] = fieldTypes;
        ViewData["Stages"] = stages;
        ViewData["SelectedCollection"] = collection;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;

        return View("Index", collections);
    }


    [HttpGet]
    public async Task<IActionResult> NewRecord(string id)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var userId = _userSessionService.GetUserId();
        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();

        var collection = await _collectionService.GetCollectionByIdAsync(id);
        if (collection == null || collection.UserId != userId)
        {
            return RedirectToAction("Index");
        }

        var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);
        var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
        var stages = await _stageService.GetAllStagesAsync();

        // Log collection.Fields as JSON
        _logger.LogInformation("=== collection.Fields (JSON) ===");
        if (collection.Fields != null)
        {
            var fieldsJson = JsonSerializer.Serialize(collection.Fields, new JsonSerializerOptions { WriteIndented = true });
            _logger.LogInformation($"Total fields: {collection.Fields.Count}");
            _logger.LogInformation($"JSON:\n{fieldsJson}");
            
            // Log class structure
            _logger.LogInformation("=== collection.Fields (Class Structure) ===");
            _logger.LogInformation($"Type: {collection.Fields.GetType().FullName}");
            _logger.LogInformation($"Is List: {collection.Fields is List<CollectionField>}");
            if (collection.Fields.Count > 0)
            {
                var firstField = collection.Fields[0];
                _logger.LogInformation($"First Field Type: {firstField.GetType().FullName}");
                _logger.LogInformation($"First Field Properties: {string.Join(", ", firstField.GetType().GetProperties().Select(p => p.Name))}");
            }
        }
        else
        {
            _logger.LogInformation("collection.Fields is NULL");
        }

        // Filter fields: exclude system fields and fields with type "system"
        var filteredFields = collection.Fields?
            .Where(f => f.Name != "id" && 
                        f.Name != "created" && 
                        f.Name != "updated" && 
                        f.Name != "createdAt" && 
                        f.Name != "updatedAt" && 
                        f.Name != "order" &&
                        FieldTypeEnumExtensions.FromString(f.Type) != FieldTypeEnum.System)
            .OrderBy(f => f.Order)
            .ToList() ?? new List<CollectionField>();

        // Log filteredFields as JSON
        _logger.LogInformation("=== filteredFields (JSON) ===");
        _logger.LogInformation($"Total filtered fields: {filteredFields.Count}");
        var filteredFieldsJson = JsonSerializer.Serialize(filteredFields, new JsonSerializerOptions { WriteIndented = true });
        _logger.LogInformation($"JSON:\n{filteredFieldsJson}");
        
        // Log class structure
        _logger.LogInformation("=== filteredFields (Class Structure) ===");
        _logger.LogInformation($"Type: {filteredFields.GetType().FullName}");
        _logger.LogInformation($"Is List: {filteredFields is List<CollectionField>}");
        if (filteredFields.Count > 0)
        {
            var firstField = filteredFields[0];
            _logger.LogInformation($"First Field Type: {firstField.GetType().FullName}");
            _logger.LogInformation($"First Field Properties: {string.Join(", ", firstField.GetType().GetProperties().Select(p => p.Name))}");
        }

        ViewData["Collections"] = collections;
        ViewData["FieldTypes"] = fieldTypes;
        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;
        ViewData["FilteredFields"] = filteredFields;

        return View(collection);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Consumes("application/x-www-form-urlencoded", "multipart/form-data")]
    public async Task<IActionResult> CreateRecord(string collectionId, IFormCollection form)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var stage = _userSessionService.GetStage();

            var collection = await _collectionService.GetCollectionByIdAsync(collectionId);
            if (collection == null || collection.UserId != userId)
            {
                TempData["ErrorMessage"] = "Collection not found";
                return RedirectToAction("Index");
            }

            var recordData = new BsonDocument();
            
            // Get all form field names (excluding system fields)
            var formFieldNames = form.Keys.Where(k => 
                k != "collectionId" && 
                k != "__RequestVerificationToken").ToHashSet();
            
            // Process all collection fields to ensure checkboxes are handled correctly
            if (collection.Fields != null)
            {
                foreach (var collectionField in collection.Fields)
                {
                    var fieldName = collectionField.Name;
                    var fieldType = collectionField.Type?.ToLower();
                    
                    // Skip system fields
                    if (fieldName == "id" || fieldName == "created" || fieldName == "updated" || 
                        fieldName == "createdAt" || fieldName == "updatedAt" || fieldType == "system")
                        continue;
                    
                    if (fieldType == "bool" || fieldType == "boolean")
                    {
                        // For checkboxes, if not in form, it means false
                        recordData[fieldName] = formFieldNames.Contains(fieldName);
                    }
                    else if (formFieldNames.Contains(fieldName))
                    {
                        var value = form[fieldName].ToString();
                        
                        if (string.IsNullOrEmpty(value))
                        {
                            recordData[fieldName] = BsonNull.Value;
                        }
                        else if (fieldType == "number")
                        {
                            if (int.TryParse(value, out var intValue))
                                recordData[fieldName] = intValue;
                            else if (double.TryParse(value, out var doubleValue))
                                recordData[fieldName] = doubleValue;
                            else
                                recordData[fieldName] = value;
                        }
                        else
                        {
                            recordData[fieldName] = value;
                        }
                    }
                    else
                    {
                        // Field not in form, set as null
                        recordData[fieldName] = BsonNull.Value;
                    }
                }
            }
            
            // Also process any additional fields that might be in the form but not in collection definition
            foreach (var field in form)
            {
                if (field.Key == "collectionId" || field.Key == "__RequestVerificationToken")
                    continue;
                
                // Skip if already processed
                if (recordData.Contains(field.Key))
                    continue;
                
                var value = field.Value.ToString();
                if (string.IsNullOrEmpty(value))
                {
                    recordData[field.Key] = BsonNull.Value;
                }
                else
                {
                    recordData[field.Key] = value;
                }
            }

            // Create collection name with stage prefix: {stage}_{collectionName}
            var collectionName = $"{stage}_{collection.Name}";

            var created = await _recordService.CreateRecordAsync(collectionName, recordData, userId, stage);
            
            TempData["SuccessMessage"] = "Record created successfully!";
            return RedirectToAction("Collection", new { id = collectionId });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating record");
            TempData["ErrorMessage"] = $"Error creating record: {ex.Message}";
            return RedirectToAction("NewRecord", new { id = collectionId });
        }
    }

    [HttpPost]
    [Route("api/Collections/CreateRecord")]
    [Consumes("application/json")]
    public async Task<IActionResult> CreateRecordApi([FromBody] CreateRecordRequest request)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Not authenticated" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var stage = _userSessionService.GetStage();

            var collection = await _collectionService.GetCollectionByIdAsync(request.CollectionId);
            if (collection == null || collection.UserId != userId)
            {
                return Json(new { success = false, message = "Collection not found" });
            }

            var recordData = new BsonDocument();
            foreach (var field in request.Data)
            {
                if (field.Value == null)
                {
                    recordData[field.Key] = BsonNull.Value;
                }
                else
                {
                    recordData[field.Key] = BsonValue.Create(field.Value);
                }
            }

            // Create collection name with stage prefix: {stage}_{collectionName}
            var collectionName = $"{stage}_{collection.Name}";

            var created = await _recordService.CreateRecordAsync(collectionName, recordData, userId, stage);
            return Json(new { success = true, record = created });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating record");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetRecords(string collectionId)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Not authenticated" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var collection = await _collectionService.GetCollectionByIdAsync(collectionId);
            
            if (collection == null || collection.UserId != userId)
            {
                return Json(new { success = false, message = "Collection not found" });
            }

            var stage = _userSessionService.GetStage();
            // Create collection name with stage prefix: {stage}_{collectionName}
            var collectionName = $"{stage}_{collection.Name}";

            var records = await _recordService.GetRecordsAsync(collectionName);
            
            // Convert BsonDocument to JSON-serializable format
            var recordsList = records.Select(r => 
            {
                var dict = new Dictionary<string, object?>();
                foreach (var element in r)
                {
                    var key = element.Name;
                    var value = element.Value;
                    
                    // Handle different BSON types
                    if (value.IsBsonNull)
                    {
                        dict[key] = null;
                    }
                    else if (value.IsBsonArray)
                    {
                        dict[key] = value.AsBsonArray.Select(v => ConvertBsonValue(v)).ToList();
                    }
                    else if (value.IsBsonDocument)
                    {
                        var nestedDict = new Dictionary<string, object?>();
                        foreach (var nestedElement in value.AsBsonDocument)
                        {
                            nestedDict[nestedElement.Name] = ConvertBsonValue(nestedElement.Value);
                        }
                        dict[key] = nestedDict;
                    }
                    else
                    {
                        dict[key] = ConvertBsonValue(value);
                    }
                }
                return dict;
            }).ToList();

            return Json(new { success = true, records = recordsList });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting records");
            return Json(new { success = false, message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> Logs()
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;

        return View();
    }

    [HttpGet]
    public async Task<IActionResult> Settings()
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        var stage = _userSessionService.GetStage();
        var userEmail = _userSessionService.GetUserEmail();
        var stages = await _stageService.GetAllStagesAsync();

        ViewData["Stages"] = stages;
        ViewData["Stage"] = stage;
        ViewData["UserEmail"] = userEmail;

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> UpdateStage(string stage)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        if (!string.IsNullOrEmpty(stage))
        {
            var stageObj = await _stageService.GetStageByKeyAsync(stage.ToLower());
            if (stageObj != null)
            {
                _userSessionService.SetStage(stageObj.Key);
                _logger.LogInformation("Stage updated to {Stage}", stageObj.Key);
            }
        }

        return RedirectToAction("Index");
    }

    [HttpGet]
    [Route("Collections/GetCollectionsByStage")]
    public async Task<IActionResult> GetCollectionsByStage([FromQuery] string stage)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();

            // Get collections for the specified stage
            var collections = await _collectionService.GetCollectionsByUserAsync(userId, stage);

            // Return simplified collection data
            var collectionsData = collections.Select(c => new
            {
                name = c.Name,
                label = c.Label,
                type = c.Type,
                fieldCount = c.Fields?.Count ?? 0
            }).ToList();

            return Json(new { success = true, collections = collectionsData });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting collections for stage {Stage}", stage);
            return Json(new { success = false, message = "Error loading collections: " + ex.Message });
        }
    }

    [HttpGet]
    [Route("Collections/{id}/Migrate")]
    public async Task<IActionResult> Migrate(string id)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return RedirectToAction("Login", "Auth");
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var currentStage = _userSessionService.GetStage();
            var userEmail = _userSessionService.GetUserEmail();

            var collection = await _collectionService.GetCollectionByIdAsync(id);
            if (collection == null)
            {
                return NotFound();
            }

            if (collection.UserId != userId)
            {
                return Unauthorized();
            }

            var collections = await _collectionService.GetCollectionsByUserAsync(userId, currentStage);
            var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
            var stages = await _stageService.GetAllStagesAsync();

            ViewData["Collections"] = collections;
            ViewData["FieldTypes"] = fieldTypes;
            ViewData["Stages"] = stages;
            ViewData["CurrentStage"] = currentStage;
            ViewData["UserEmail"] = userEmail;

            return View(collection);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading migration page for collection {Id}", id);
            return RedirectToAction("Index");
        }
    }

    [HttpGet]
    [Route("Collections/{id}/CheckMigration")]
    public async Task<IActionResult> CheckMigration(string id, [FromQuery] string targetStage, [FromQuery] string? collectionName = null)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var currentStage = _userSessionService.GetStage();

            // Get source collection
            var sourceCollection = await _collectionService.GetCollectionByIdAsync(id);
            if (sourceCollection == null)
            {
                return Json(new { success = false, message = "Source collection not found" });
            }

            // Check if user owns the collection
            if (sourceCollection.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Use provided collection name or source collection name
            var targetCollectionName = string.IsNullOrWhiteSpace(collectionName) ? sourceCollection.Name : collectionName;

            // Check if collection exists in target stage
            var targetCollection = await _collectionService.GetCollectionByNameAsync(
                targetCollectionName, 
                userId, 
                targetStage
            );

            // Prepare source fields
            var sourceFields = sourceCollection.Fields
                .Select(f => new { 
                    name = f.Name, 
                    type = f.Type,
                    label = f.Label,
                    order = f.Order
                })
                .OrderBy(f => f.order)
                .ToList();

            if (targetCollection == null)
            {
                // Collection doesn't exist in target stage
                return Json(new { 
                    success = true, 
                    exists = false,
                    sourceFields = sourceFields,
                    targetFields = new List<object>(),
                    newFields = sourceFields,
                    message = "Collection does not exist in target stage. It will be created."
                });
            }

            // Collection exists - find new fields
            var targetFieldNames = targetCollection.Fields.Select(f => f.Name).ToHashSet();
            
            var targetFields = targetCollection.Fields
                .Select(f => new { 
                    name = f.Name, 
                    type = f.Type,
                    label = f.Label,
                    order = f.Order
                })
                .OrderBy(f => f.order)
                .ToList();

            var newFields = sourceCollection.Fields
                .Where(f => !targetFieldNames.Contains(f.Name))
                .Select(f => new { 
                    name = f.Name, 
                    type = f.Type,
                    label = f.Label,
                    order = f.Order
                })
                .OrderBy(f => f.order)
                .ToList();

            return Json(new { 
                success = true, 
                exists = true,
                sourceFields = sourceFields,
                targetFields = targetFields,
                newFields = newFields,
                message = newFields.Any() 
                    ? $"Collection exists. {newFields.Count} new field(s) will be added." 
                    : "Collection exists and is identical."
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking migration for collection {Id}", id);
            return Json(new { success = false, message = "Error checking migration: " + ex.Message });
        }
    }

    [HttpPost]
    [Route("Collections/{id}/Migrate")]
    public async Task<IActionResult> Migrate(string id, [FromBody] MigrateRequest request)
    {
        if (!_userSessionService.IsAuthenticated())
        {
            return Json(new { success = false, message = "Unauthorized" });
        }

        try
        {
            var userId = _userSessionService.GetUserId();
            var currentStage = _userSessionService.GetStage();

            // Get source collection
            var sourceCollection = await _collectionService.GetCollectionByIdAsync(id);
            if (sourceCollection == null)
            {
                return Json(new { success = false, message = "Source collection not found" });
            }

            // Check if user owns the collection
            if (sourceCollection.UserId != userId)
            {
                return Json(new { success = false, message = "Unauthorized" });
            }

            // Validate target stage
            var targetStageObj = await _stageService.GetStageByKeyAsync(request.TargetStage);
            if (targetStageObj == null)
            {
                return Json(new { success = false, message = "Invalid target stage" });
            }

            // Use provided collection name or source collection name
            var targetCollectionName = string.IsNullOrWhiteSpace(request.CollectionName) 
                ? sourceCollection.Name 
                : request.CollectionName;

            // Check if collection exists in target stage
            var targetCollection = await _collectionService.GetCollectionByNameAsync(
                targetCollectionName, 
                userId, 
                request.TargetStage
            );

            if (targetCollection == null)
            {
                // Create new collection in target stage
                var newCollection = new Collection
                {
                    Name = targetCollectionName,
                    Label = sourceCollection.Label,
                    Type = sourceCollection.Type,
                    Fields = sourceCollection.Fields,
                    UserId = userId,
                    Stage = request.TargetStage
                };

                await _collectionService.CreateCollectionAsync(newCollection);
                
                return Json(new { 
                    success = true, 
                    message = $"Collection '{targetCollectionName}' created successfully in {targetStageObj.Label} stage."
                });
            }
            else
            {
                // Update existing collection with new fields
                var targetFieldNames = targetCollection.Fields.Select(f => f.Name).ToHashSet();
                var newFields = sourceCollection.Fields
                    .Where(f => !targetFieldNames.Contains(f.Name))
                    .ToList();

                if (newFields.Any())
                {
                    targetCollection.Fields.AddRange(newFields);
                    await _collectionService.UpdateCollectionAsync(targetCollection.Id, targetCollection);
                    
                    return Json(new { 
                        success = true, 
                        message = $"{newFields.Count} new field(s) added to collection '{targetCollectionName}' in {targetStageObj.Label} stage."
                    });
                }
                else
                {
                    return Json(new { 
                        success = true, 
                        message = "Collection already exists and is identical. No changes made."
                    });
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error migrating collection {Id}", id);
            return Json(new { success = false, message = "Error migrating collection: " + ex.Message });
        }
    }

    private object? ConvertBsonValue(BsonValue value)
    {
        if (value.IsBsonNull)
            return null;
        if (value.IsBoolean)
            return value.AsBoolean;
        if (value.IsInt32)
            return value.AsInt32;
        if (value.IsInt64)
            return value.AsInt64;
        if (value.IsDouble)
            return value.AsDouble;
        if (value.IsString)
            return value.AsString;
        if (value.IsBsonDateTime)
            return value.AsBsonDateTime.ToUniversalTime();
        if (value.IsObjectId)
            return value.AsObjectId.ToString();
        return value.ToString();
    }
}

public class CreateCollectionRequest
{
    public string Name { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public string? Type { get; set; }
    public List<CollectionField>? Fields { get; set; }
}

public class CreateRecordRequest
{
    public string CollectionId { get; set; } = string.Empty;
    public Dictionary<string, object> Data { get; set; } = new();
}

public class MigrateRequest
{
    public string TargetStage { get; set; } = string.Empty;
    public string? CollectionName { get; set; }
}
