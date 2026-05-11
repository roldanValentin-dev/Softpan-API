using Softpan.Domain.Entities;

namespace Softpan.Domain.Interfaces;

public interface IAuditRepository
{
    Task<AuditLog> CreateAsync(AuditLog auditLog);
    Task<List<AuditLog>> GetAllAsync(int page, int pageSize);
    Task<List<AuditLog>> GetByUserIdAsync(string userId, int page, int pageSize);
    Task<List<AuditLog>> GetByEntityAsync(string entity, int? entityId, int page, int pageSize);
}
