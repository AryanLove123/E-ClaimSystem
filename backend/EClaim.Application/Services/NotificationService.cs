using EClaim.Application.DTOs.Notifications;
using EClaim.Application.Interfaces;
using EClaim.Domain.Entities;
using EClaim.Domain.Enums;
using EClaim.Domain.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace EClaim.Application.Services;

public class NotificationService : INotificationService
{
    private IEClaimDbContext _db;
    
    public NotificationService(IEClaimDbContext db)
    {
        _db = db;
    }
    public async Task NotifyAsync(User user, string subject, string message, int? claimId = null, CancellationToken ct = default)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = user.Id,
            Type = NotificationType.InApp,
            Subject = subject,
            Message = message,
            IsRead = false,
            SentSuccessfully = true,
            ClaimId = claimId
        });
        await _db.SaveChangesAsync(ct);
    }
    public async Task<NotificationSummaryDto> GetSummaryAsync(int userId, int take = 10, CancellationToken ct = default)
    {
        var mine = _db.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && n.Type == NotificationType.InApp);
        
        var unreadCount = await mine.CountAsync(n => !n.IsRead, ct);

        var items = await mine
            .OrderByDescending(n => n.CreatedAt)
            .ThenByDescending(n => n.Id)
            .Take(take)
            .ToListAsync(ct);

        return new NotificationSummaryDto
        {
            UnreadCount = unreadCount,
            Items = items.Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Subject = n.Subject,
                Message = n.Message,
                IsRead = n.IsRead,
                ClaimId = n.ClaimId,
                CreatedAt = n.CreatedAt
            }).ToList()
        };
    }
    public async Task MarkAsReadAsync(int userId, int notificationId, CancellationToken ct = default)
    {
        var notification = await _db.Notifications.FirstOrDefaultAsync(
                n => n.Id == notificationId && n.UserId == userId && n.Type == NotificationType.InApp , ct)
                ?? throw new NotFoundException("Notification", notificationId);
        
        if(notification.IsRead) return;

        notification.IsRead = true;
        await _db.SaveChangesAsync(ct);
    }
    public async Task MarkAllAsReadAsync(int userId, CancellationToken ct = default)
    {
        var unread = await _db.Notifications
            .Where(n => n.UserId == userId && n.Type == NotificationType.InApp && !n.IsRead)
            .ToListAsync(ct);
        
        if(unread.Count == 0) return;

        foreach (var n in unread)
        {
            n.IsRead = true;
        }
        await _db.SaveChangesAsync(ct);
    }
}