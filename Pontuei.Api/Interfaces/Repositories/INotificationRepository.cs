using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>notification</c> table.
/// All queries are scoped to a specific user.
/// </summary>
public interface INotificationRepository
{
    /// <summary>
    /// Returns the notification with the given <paramref name="notificationId"/>,
    /// including <c>Transaction</c> and <c>LoyaltyProgram</c> navigation properties,
    /// or <c>null</c> when not found.
    /// </summary>
    Task<Notification?> GetByIdAsync(Guid notificationId, bool verifyDeleted = true);

    /// <summary>
    /// Returns all notifications for the given user, ordered by creation time descending
    /// (most recent first). Includes <c>LoyaltyProgram</c> for badge rendering.
    /// </summary>
    IQueryable<Notification> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Returns the count of unread notifications for the given user.
    /// Used to render the notification badge on the bottom navigation bar.
    /// </summary>
    Task<int> CountUnreadByUserIdAsync(Guid userId);

    /// <summary>
    /// Persists a new notification row and returns the saved entity.
    /// Called internally by service methods that produce side-effect notifications
    /// (e.g., transaction status change, overdue alert).
    /// </summary>
    Task<Notification> CreateAsync(NotificationDto dto, string createdBy);

    /// <summary>
    /// Marks a single notification as read by setting <c>notification_is_read = true</c>
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task MarkAsReadAsync(Notification notification, string readBy);

    /// <summary>
    /// Marks all unread notifications for the given user as read.
    /// Returns the number of rows updated.
    /// </summary>
    Task<int> MarkAllAsReadAsync(Guid userId, string readBy);
}
