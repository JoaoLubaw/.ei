using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for notification management.
/// Handles retrieval, read-state updates, and programmatic creation of
/// notifications triggered by transaction and system events.
/// </summary>
public interface INotificationService
{
    // ── Queries ───────────────────────────────────────────────────────────

    /// <summary>
    /// Returns all notifications for the authenticated user, ordered by
    /// creation time descending, as shown on the "Notificações" screen.
    /// </summary>
    Task<ApiResult<GetNotificationsResponseDto>> GetByUserIdAsync(Guid userId, GetNotificationsRequestDto dto, Guid currentUserId);

    // ── Read-state mutations ──────────────────────────────────────────────

    /// <summary>
    /// Marks a single notification as read.
    /// Typically triggered when the user taps a notification row.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the notification is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the notification does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<NotificationDto>> MarkAsReadAsync(Guid userId, Guid notificationId, Guid currentUserId);

    /// <summary>
    /// Marks all of the user's unread notifications as read.
    /// Triggered by "Marcar todas as notificações como lidas" at the bottom of the screen.
    /// Returns a summary with the number of rows updated.
    /// </summary>
    Task<ApiResult<GetNotificationsResponseDto>> MarkAllAsReadAsync(GetNotificationsRequestDto requestDto, Guid userId, Guid currentUserId);

    /// <summary>
    /// Returns the count of unread notifications for the user.
    /// </summary>
    Task<ApiResult<int>> GetUnreadCountAsync(Guid userId, Guid currentUserId);
}
