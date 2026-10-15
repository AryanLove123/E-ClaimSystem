namespace EClaim.Application.Interfaces;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entity, string entityId, string? oldValue = null, string? newValue = null, CancellationToken ct = default);
}
