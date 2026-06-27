using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class NotificationRepository : BaseRepository, INotificationRepository
{
    public NotificationRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the notification with the given <paramref name="notificationId"/>,
    /// including <c>Transaction</c> and <c>LoyaltyProgram</c> navigation properties,
    /// or <c>null</c> when not found.
    /// </summary>
    public async Task<Notification?> GetByIdAsync(Guid notificationId, bool verifyDeleted = true)
    {
        return await _dbContext.Notifications
            .Include(n => n.Transaction)
            .Include(n => n.LoyaltyProgram)
                .FirstOrDefaultAsync(n => n.NotificationId == notificationId && (!verifyDeleted || !n.IsDeleted));
    }

    /// <summary>
    /// Returns all notifications for the given user, ordered by creation time descending
    /// (most recent first). Includes <c>LoyaltyProgram</c> for badge rendering.
    /// </summary>
    public IQueryable<Notification> GetByUserIdAsync(Guid userId)
    {
        return _dbContext.Notifications
            .Include(n => n.Transaction)
            .Include(n => n.LoyaltyProgram)
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreationTime);
    }

    /// <summary>
    /// Returns the count of unread notifications for the given user.
    /// Used to render the notification badge on the bottom navigation bar.
    /// </summary>
    public async Task<int> CountUnreadByUserIdAsync(Guid userId)
    {
        return await _dbContext.Notifications
            .Where(n => n.UserId == userId && !n.NotificationIsRead && !n.IsDeleted)
            .CountAsync();
    }

    /// <summary>
    /// Persists a new notification row and returns the saved entity.
    /// Called internally by service methods that produce side-effect notifications
    /// (e.g., transaction status change, overdue alert).
    /// </summary>
    public async Task<Notification> CreateAsync(NotificationDto dto, string createdBy)
    {
        Notification notification = new Notification
        {
            NotificationId = Guid.NewGuid(),
            UserId = dto.UserId,
            TransactionId = dto.TransactionId,
            LoyaltyProgramId = dto.LoyaltyProgramId,
            NotificationPointsAmount = dto.NotificationPointsAmount,
            NotificationMessage = dto.NotificationMessage,
            NotificationIsRead = false,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.Notifications.Add(notification);

        return notification;
    }

    /// <summary>
    /// Marks a single notification as read by setting <c>notification_is_read = true</c>
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public async Task MarkAsReadAsync(Notification notification, string readBy)
    {
        _dbContext.Attach(notification);

        notification.NotificationIsRead = true;
        _dbContext.Entry(notification).Property(n => n.NotificationIsRead).IsModified = true;

        notification.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(notification).Property(n => n.UpdateTime).IsModified = true;

        notification.UpdateUser = readBy;
        _dbContext.Entry(notification).Property(n => n.UpdateUser).IsModified = true;
    }

    /// <summary>
    /// Marks all unread notifications for the given user as read.
    /// Returns the number of rows updated.
    /// </summary>
    public async Task<int> MarkAllAsReadAsync(Guid userId, string readBy)
    {
        return await _dbContext.Notifications
                .Where(n => n.UserId == userId && !n.NotificationIsRead && !n.IsDeleted)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(n => n.NotificationIsRead, true)
                    .SetProperty(n => n.UpdateTime, DateTime.UtcNow)
                    .SetProperty(n => n.UpdateUser, readBy));
    }
}