namespace Softpan.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(string userId, string userEmail, string action, string entity, int? entityId = null, string? details = null, string ipAddress = "");
    Task<List<AuditLogDto>> GetLogsAsync(int page = 1, int pageSize = 50);
    Task<List<AuditLogDto>> GetLogsByUserAsync(string userId, int page = 1, int pageSize = 50);
    Task<List<AuditLogDto>> GetLogsByEntityAsync(string entity, int? entityId = null, int page = 1, int pageSize = 50);
}

public class AuditLogDto
{
    public int Id { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string Entity { get; set; } = string.Empty;
    public int? EntityId { get; set; }
    public string? Details { get; set; }
    public string IpAddress { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; }
}
