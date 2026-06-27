using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Pontuei.Api.Controllers;

[SwaggerTag("Notification management")]
[Authorize]
public class NotificationController : PontueiControllerBase
{
    private readonly INotificationService _notificationService;

    public NotificationController(
        INotificationService notificationService,
        ILogger<NotificationController> logger) : base(logger)
    {
        _notificationService = notificationService;
    }

    // ── User-scoped routes (/users/{userId}/notifications) ────────────────

    /// <summary>
    /// Returns all notifications for the given user, ordered by creation time descending.
    /// </summary>
    /// <remarks>
    /// Displayed on the "Notificações" screen.
    /// </remarks>
    /// <response code="200">Returns the notification list.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNotificationsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet("users/{userId:guid}/notifications")]
    public async Task<ActionResult<GetNotificationsResponseDto>> GetNotifications(
        [FromRoute] Guid userId,
        [FromQuery] GetNotificationsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<GetNotificationsResponseDto> apiResult = await _notificationService.GetByUserIdAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetNotifications));
        }
    }

    /// <summary>
    /// Marks all of the user's unread notifications as read.
    /// </summary>
    /// <remarks>
    /// Triggered by "Marcar todas as notificações como lidas" at the bottom of the notifications screen.
    /// Returns an updated summary with the number of rows affected.
    /// </remarks>
    /// <response code="200">All notifications marked as read. Returns the updated notification summary.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetNotificationsResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPatch("users/{userId:guid}/notifications/read-all")]
    public async Task<ActionResult<GetNotificationsResponseDto>> MarkAllAsRead(
        [FromRoute] Guid userId,
        [FromBody] GetNotificationsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("MarkAllAsRead called for user {UserId}", userId);

        try
        {
            ApiResult<GetNotificationsResponseDto> apiResult = await _notificationService.MarkAllAsReadAsync(requestDto, userId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(MarkAllAsRead));
        }
    }

    // ── Notification-scoped routes (/notifications/{id}) ─────────────────

    /// <summary>
    /// Returns the count of unread notifications for the authenticated user.
    /// </summary>
    /// <remarks>
    /// Used to drive the badge on the home screen bell icon.
    /// </remarks>
    /// <response code="200">Returns the unread count.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(int))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet("notifications/unread-count")]
    public async Task<ActionResult<int>> GetUnreadCount()
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<int> apiResult = await _notificationService.GetUnreadCountAsync(currentUserId.Value, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetUnreadCount));
        }
    }

    /// <summary>
    /// Marks a single notification as read.
    /// </summary>
    /// <remarks>
    /// Typically triggered when the user taps a notification row.
    /// </remarks>
    /// <response code="200">Notification marked as read. Returns the updated notification.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Notification does not belong to the requesting user.</response>
    /// <response code="404">Notification not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(NotificationDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPatch("notifications/{notificationId:guid}/read")]
    public async Task<ActionResult<NotificationDto>> MarkAsRead([FromRoute] Guid notificationId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<NotificationDto> apiResult = await _notificationService.MarkAsReadAsync(currentUserId.Value, notificationId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(MarkAsRead));
        }
    }
}