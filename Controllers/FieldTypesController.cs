using Microsoft.AspNetCore.Mvc;
using genial_dotnet_crm.Models;
using genial_dotnet_crm.Services;

namespace genial_dotnet_crm.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FieldTypesController : ControllerBase
{
    private readonly ILogger<FieldTypesController> _logger;
    private readonly IFieldTypeService _fieldTypeService;

    public FieldTypesController(
        ILogger<FieldTypesController> logger,
        IFieldTypeService fieldTypeService)
    {
        _logger = logger;
        _fieldTypeService = fieldTypeService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var fieldTypes = await _fieldTypeService.GetAllFieldTypesAsync();
            return Ok(fieldTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field types");
            return StatusCode(500, new { message = "Error getting field types" });
        }
    }

    [HttpGet("{type}")]
    public async Task<IActionResult> GetByType(string type)
    {
        try
        {
            var fieldType = await _fieldTypeService.GetFieldTypeByTypeAsync(type);
            if (fieldType == null)
            {
                return NotFound(new { message = "Field type not found" });
            }
            return Ok(fieldType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting field type");
            return StatusCode(500, new { message = "Error getting field type" });
        }
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] FieldType fieldType)
    {
        try
        {
            var created = await _fieldTypeService.CreateFieldTypeAsync(fieldType);
            return CreatedAtAction(nameof(GetByType), new { type = created.Type }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating field type");
            return StatusCode(500, new { message = "Error creating field type" });
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(string id, [FromBody] FieldType fieldType)
    {
        try
        {
            var updated = await _fieldTypeService.UpdateFieldTypeAsync(id, fieldType);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating field type");
            return StatusCode(500, new { message = "Error updating field type" });
        }
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(string id)
    {
        try
        {
            var deleted = await _fieldTypeService.DeleteFieldTypeAsync(id);
            if (!deleted)
            {
                return NotFound(new { message = "Field type not found" });
            }
            return Ok(new { message = "Field type deleted successfully" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting field type");
            return StatusCode(500, new { message = "Error deleting field type" });
        }
    }
}


