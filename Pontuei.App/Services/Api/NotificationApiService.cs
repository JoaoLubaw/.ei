using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// Covers all notification-related API endpoints:
///
///   GET    /users/{userId}/notifications
///   PATCH  /users/{userId}/notifications/read-all
///   GET    /notifications/unread-count
///   PATCH  /notifications/{notificationId}/read
/// </summary>
public class NotificationApiService
{
    private readonly ApiClient _api;

    public NotificationApiService(ApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Lists all notifications for a user, with optional filtering and pagination. 
    /// </summary>
    public Task<ApiResponse<GetNotificationsResponseDto>> GetNotificationsAsync(
        Guid userId,
        GetNotificationsRequestDto? request = null)
    {
        string url = ApiClient.BuildQueryString($"users/{userId}/notifications", request);
        return _api.GetAsync<GetNotificationsResponseDto>(url);
    }

    /// <summary>
    /// Marks all unread notifications for a user as read.
    /// Returns the updated summary with the count of affected rows.
    /// </summary>
    public Task<ApiResponse<GetNotificationsResponseDto>> MarkAllAsReadAsync(
        Guid userId,
        GetNotificationsRequestDto request)
        => _api.PatchAsync<GetNotificationsResponseDto>(
            $"users/{userId}/notifications/read-all", request);

    /// <summary>
    /// Returns the count of unread notifications (badge on the bell icon in the home screen).
    /// Read from Redis in the API — fast response.
    /// </summary>
    public Task<ApiResponse<int>> GetUnreadCountAsync()
        => _api.GetAsync<int>("notifications/unread-count");

    /// <summary>
    /// Marks an individual notification as read when tapped in the list.
    /// </summary>
    public Task<ApiResponse<NotificationDto>> MarkAsReadAsync(Guid notificationId)
        => _api.PatchAsync<NotificationDto>($"notifications/{notificationId}/read");

}