using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

/// <summary>
/// 
/// </summary>
public class NotificationService : INotificationService
{
    private readonly INotificationRepository _notificationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        INotificationRepository notificationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<NotificationService> logger
    )
    {
        _notificationRepository = notificationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Returns all notifications for the authenticated user, ordered by
    /// creation time descending, as shown on the "Notificações" screen.
    /// </summary>
    public async Task<ApiResult<GetNotificationsResponseDto>> GetByUserIdAsync(Guid userId, GetNotificationsRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<GetNotificationsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        IQueryable<Notification> query = _notificationRepository.GetByUserIdAsync(userId);
        query = ApplyFilters(query, dto);

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<Notification> configurations = await query
            .Skip(skip)
            .Take(dto.Size)
            .ToListAsync();

        List<NotificationDto> configurationsDtos = configurations.Adapt<List<NotificationDto>>();

        _logger.LogInformation("Retrieved {Count} configurations for page {Page} with size {Size}. Total elements: {TotalElements}, Total pages: {TotalPages} For user {UserId}.",
            configurationsDtos.Count, dto.Page, dto.Size, totalElements, totalPages, currentUserId);

        return new ApiResult<GetNotificationsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetNotificationsResponseDto
            {
                Notifications = configurationsDtos,
                Page = dto.Page,
                Size = dto.Size,
                UnreadCount = await _notificationRepository.CountUnreadByUserIdAsync(userId),
                TotalElements = totalElements,
                TotalPages = totalPages
            }
        );
    }

    private IQueryable<Notification> ApplyFilters(IQueryable<Notification> query, GetNotificationsRequestDto dto)
    {
        if (dto.Filters == null)
            return query;

        if (dto.Filters.NotificationId.HasValue)
        {
            query = query.Where(n => n.NotificationId == dto.Filters.NotificationId.Value);
        }

        if (dto.Filters.TransactionId.HasValue)
        {
            query = query.Where(n => n.TransactionId == dto.Filters.TransactionId.Value);
        }

        if (dto.Filters.LoyaltyProgramId.HasValue)
        {
            query = query.Where(n => n.LoyaltyProgramId == dto.Filters.LoyaltyProgramId.Value);
        }

        if (dto.Filters.NotificationIsRead.HasValue)
        {
            query = query.Where(n => n.NotificationIsRead == dto.Filters.NotificationIsRead.Value);
        }

        return query;
    }

    // ── Read-state mutations ──────────────────────────────────────────────

    /// <summary>
    /// Marks a single notification as read.
    /// Typically triggered when the user taps a notification row.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the notification is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the notification does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<NotificationDto>> MarkAsReadAsync(Guid userId, Guid notificationId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<NotificationDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        Notification? notification = await _notificationRepository.GetByIdAsync(notificationId);

        if (notification == null)
        {
            _logger.LogWarning("Notification with ID {NotificationId} not found for user {UserId}.", notificationId, currentUserId);

            return new ApiResult<NotificationDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (notification.UserId != userId)
        {
            _logger.LogWarning("User with ID {UserId} attempted to mark notification {NotificationId} as read, but it belongs to user {OwnerId}.", currentUserId, notificationId, notification.UserId);

            return new ApiResult<NotificationDto>(
                InternalResultCode.NOT_ALLOWED_TO_CREATE_OR_EDIT_ITEM,
                HttpStatusCode.Forbidden,
                null
            );
        }

        await _notificationRepository.MarkAsReadAsync(notification, loggedUser.UserName);
        await _unitOfWork.CommitAsync();

        return new ApiResult<NotificationDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            notification.Adapt<NotificationDto>()
        );

    }

    /// <summary>
    /// Marks all of the user's unread notifications as read.
    /// Triggered by "Marcar todas as notificações como lidas" at the bottom of the screen.
    /// Returns a summary with the number of rows updated.
    /// </summary>
    public async Task<ApiResult<GetNotificationsResponseDto>> MarkAllAsReadAsync(GetNotificationsRequestDto requestDto, Guid userId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<GetNotificationsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        int updatedCount = await _notificationRepository.MarkAllAsReadAsync(userId, loggedUser.UserName);
        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Marked all notifications as read for user {UserId}. Total updated: {UpdatedCount}.", currentUserId, updatedCount);

        IQueryable<Notification> query = _notificationRepository.GetByUserIdAsync(userId);
        query = ApplyFilters(query, requestDto);

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / requestDto.Size);
        int skip = (requestDto.Page - 1) * requestDto.Size;

        List<Notification> configurations = await query
            .Skip(skip)
            .Take(requestDto.Size)
            .ToListAsync();

        List<NotificationDto> configurationsDtos = configurations.Adapt<List<NotificationDto>>();
        return new ApiResult<GetNotificationsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetNotificationsResponseDto
            {
                Notifications = configurationsDtos,
                Page = requestDto.Page,
                Size = requestDto.Size,
                TotalElements = totalElements,
                UnreadCount = 0,
                TotalPages = totalPages
            }
        );

    }

    public async Task<ApiResult<int>> GetUnreadCountAsync(Guid userId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<int>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                0
            );
        }

        int unreadCount = await _notificationRepository.CountUnreadByUserIdAsync(userId);

        return new ApiResult<int>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            unreadCount
        );
    }

}
