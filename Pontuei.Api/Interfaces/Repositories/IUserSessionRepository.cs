using Pontuei.Api.Dtos;
using Pontuei.Api.Enums;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>user_session</c> table.
/// Each row represents an active device session with its own refresh token.
/// </summary>
public interface IUserSessionRepository
{
    /// <summary>
    /// Returns the session matching the given <paramref name="sessionId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    Task<UserSession?> GetByIdAsync(Guid sessionId);

    /// <summary>
    /// Returns the active (non-revoked, non-expired) session whose hashed
    /// refresh token matches <paramref name="refreshTokenHash"/>,
    /// or <c>null</c> when none is found.
    /// </summary>
    Task<UserSession?> GetActiveByRefreshTokenHashAsync(string refreshTokenHash);

    /// <summary>
    /// Returns all non-revoked sessions for a given user.
    /// Used when listing devices or invalidating all sessions on password change.
    /// </summary>
    IQueryable<UserSession> GetActiveByUserIdAsync(Guid userId);

    /// <summary>
    /// Persists a new session row and returns the saved entity.
    /// </summary>
    Task<UserSession> CreateAsync(
        Guid userId,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        string? deviceInfo,
        string? ipAddress,
        string? pushNotificationToken,
        string createdBy
        );

    /// <summary>
    /// Updates mutable session fields (push token, device info) and returns the saved entity.
    /// </summary>
    Task<UserSession> UpdateAsync(
        UserSession userSession,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        string? deviceInfo,
        string? ipAddress,
        string? pushNotificationToken,
        string updatedBy
    );

    /// <summary>
    /// Sets <c>user_session_is_revoked = true</c> for the given session.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task<bool> RevokeAsync(UserSession userSession, string revokedBy);

    /// <summary>
    /// Revokes all active sessions for a user at once.
    /// Called on password change or account deletion.
    /// Returns the number of rows affected.
    /// </summary>
    Task<int> RevokeAllByUserIdAsync(Guid userId, string revokedBy);

    /// <summary>
    /// Updates the push-notification token for the given session.
    /// Pass <c>null</c> in <paramref name="token"/> to clear it.
    /// </summary>
    Task SetPushNotificationTokenAsync(UserSession userSession, string? token, string updatedBy);
}
