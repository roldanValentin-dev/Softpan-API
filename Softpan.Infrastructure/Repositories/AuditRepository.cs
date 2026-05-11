using Microsoft.EntityFrameworkCore;
using Softpan.Domain.Entities;
using Softpan.Domain.Interfaces;
using Softpan.Infrastructure.Data;

namespace Softpan.Infrastructure.Repositories;

public class AuditRepository(ApplicationDbContext context) : IAuditRepository
{
    public async Task<AuditLog> CreateAsync(AuditLog auditLog)
    {
        await context.AuditLogs.AddAsync(auditLog);
        await context.SaveChangesAsync();
        return auditLog;
    }

    public async Task<List<AuditLog>> GetAllAsync(int page, int pageSize)
    {
        return await context.AuditLogs
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByUserIdAsync(string userId, int page, int pageSize)
    {
        return await context.AuditLogs
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<List<AuditLog>> GetByEntityAsync(string entity, int? entityId, int page, int pageSize)
    {
        var query = context.AuditLogs.Where(a => a.Entity == entity);

        if (entityId.HasValue)
            query = query.Where(a => a.EntityId == entityId.Value);

        return await query
            .OrderByDescending(a => a.Timestamp)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
