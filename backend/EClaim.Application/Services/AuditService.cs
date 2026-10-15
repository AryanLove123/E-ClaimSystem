using EClaim.Application.Interfaces;
using EClaim.Domain.Entities;

namespace EClaim.Application.Services;

public class AuditService : IAuditService
{
    private IEClaimDbContext _db;

    public AuditService(IEClaimDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int? userId, string action, string entity, string entityId, string? oldValue = null, string? newValue = null, CancellationToken ct = default)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            Entity = entity,
            EntityId = entityId,
            OldValue = oldValue,
            NewValue = newValue
        });
        await _db.SaveChangesAsync(ct);
    }
}
