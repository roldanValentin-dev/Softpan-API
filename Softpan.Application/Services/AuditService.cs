using Mapster;
using Softpan.Application.Interfaces;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;

namespace Softpan.Application.Services;

public class AuditService(IAuditRepository auditRepository) : IAuditService
{
    public async Task LogAsync(string userId, string userEmail, string action, string entity, int? entityId = null, string? details = null, string ipAddress = "")
    {
        var auditLog = new AuditLog
        {
            UserId = userId,
            UserEmail = userEmail,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            Details = details,
            IpAddress = ipAddress,
            Timestamp = DateTime.UtcNow
        };

        await auditRepository.CreateAsync(auditLog);
    }

    public async Task<List<AuditLogDto>> GetLogsAsync(int page = 1, int pageSize = 50)
    {
        var logs = await auditRepository.GetAllAsync(page, pageSize);
        return logs.Adapt<List<AuditLogDto>>();
    }

    public async Task<List<AuditLogDto>> GetLogsByUserAsync(string userId, int page = 1, int pageSize = 50)
    {
        var logs = await auditRepository.GetByUserIdAsync(userId, page, pageSize);
        return logs.Adapt<List<AuditLogDto>>();
    }

    public async Task<List<AuditLogDto>> GetLogsByEntityAsync(string entity, int? entityId = null, int page = 1, int pageSize = 50)
    {
        var logs = await auditRepository.GetByEntityAsync(entity, entityId, page, pageSize);
        return logs.Adapt<List<AuditLogDto>>();
    }
}
