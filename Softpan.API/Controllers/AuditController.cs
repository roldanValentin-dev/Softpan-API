using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Softpan.Application.Interfaces;

namespace Softpan.API.Controllers;

[Authorize(Roles = "Admin")]
[ApiController]
[Route("api/[controller]")]
public class AuditController(IAuditService auditService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLogs([FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await auditService.GetLogsAsync(page, pageSize);
        return Ok(logs);
    }

    [HttpGet("user/{userId}")]
    public async Task<IActionResult> GetLogsByUser(string userId, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await auditService.GetLogsByUserAsync(userId, page, pageSize);
        return Ok(logs);
    }

    [HttpGet("entity/{entity}")]
    public async Task<IActionResult> GetLogsByEntity(string entity, [FromQuery] int? entityId = null, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var logs = await auditService.GetLogsByEntityAsync(entity, entityId, page, pageSize);
        return Ok(logs);
    }
}
