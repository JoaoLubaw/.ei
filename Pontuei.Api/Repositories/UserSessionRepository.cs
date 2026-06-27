using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class UserSessionRepository : BaseRepository, IUserSessionRepository
{
    public UserSessionRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the session matching the given <paramref name="sessionId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    public async Task<UserSession?> GetByIdAsync(Guid sessionId, bool verifyActive = true)
    {
        return await _dbContext.UserSessions
            .Where(s => s.UserSessionId == sessionId && (!verifyActive || (s.UserSessionIsRevoked == false
            && !s.IsDeleted && s.UserSessionRefreshTokenExpiration > DateTime.UtcNow)))
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns the active (non-revoked, non-expired) session whose hashed
    /// refresh token matches <paramref name="refreshTokenHash"/>,
    /// or <c>null</c> when none is found.
    /// </summary>
    public async Task<UserSession?> GetActiveByRefreshTokenHashAsync(string refreshTokenHash)
    {
        return await _dbContext.UserSessions
            .Where(s => s.UserSessionRefreshToken == refreshTokenHash && s.UserSessionIsRevoked == false && !s.IsDeleted && s.UserSessionRefreshTokenExpiration > DateTime.UtcNow)
            .FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns all non-revoked sessions for a given user.
    /// Used when listing devices or invalidating all sessions on password change.
    /// </summary>
    public IQueryable<UserSession> GetActiveByUserIdAsync(Guid userId)
    {
        return _dbContext.UserSessions
            .Where(s => s.UserId == userId && s.UserSessionIsRevoked == false && !s.IsDeleted && s.UserSessionRefreshTokenExpiration > DateTime.UtcNow);
    }

    /// <summary>
    /// Persists a new session row and returns the saved entity.
    /// </summary>
    public async Task<UserSession> CreateAsync(
        Guid userId,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        string? deviceInfo,
        string? ipAddress,
        string? pushNotificationToken,
        string createdBy
    )
    {
        UserSession userSession = new UserSession
        {
            UserId = userId,
            UserSessionRefreshToken = refreshTokenHash,
            UserSessionRefreshTokenExpiration = refreshTokenExpiresAt,
            UserSessionDeviceInfo = deviceInfo,
            UserSessionIpAddress = ipAddress,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        if (!string.IsNullOrEmpty(pushNotificationToken))
        {
            userSession.UserSessionPushNotificationToken = pushNotificationToken;
        }

        _dbContext.UserSessions.Add(userSession);

        return userSession;

    }

    /// <summary>
    /// Updates mutable session fields (push token, device info) and returns the saved entity.
    /// </summary>
    public async Task<UserSession> UpdateAsync(
        UserSession userSession,
        string refreshTokenHash,
        DateTime refreshTokenExpiresAt,
        string? deviceInfo,
        string? ipAddress,
        string? pushNotificationToken,
        string updatedBy
    )
    {
        _dbContext.Attach(userSession);

        if (!string.IsNullOrEmpty(refreshTokenHash))
        {
            userSession.UserSessionRefreshToken = refreshTokenHash;
            _dbContext.Entry(userSession).Property(s => s.UserSessionRefreshToken).IsModified = true;
        }

        if (!string.IsNullOrEmpty(deviceInfo))
        {
            userSession.UserSessionDeviceInfo = deviceInfo;
            _dbContext.Entry(userSession).Property(s => s.UserSessionDeviceInfo).IsModified = true;
        }

        if (!string.IsNullOrEmpty(ipAddress))
        {
            userSession.UserSessionIpAddress = ipAddress;
            _dbContext.Entry(userSession).Property(s => s.UserSessionIpAddress).IsModified = true;
        }

        if (!string.IsNullOrEmpty(pushNotificationToken))
        {
            userSession.UserSessionPushNotificationToken = pushNotificationToken;
            _dbContext.Entry(userSession).Property(s => s.UserSessionPushNotificationToken).IsModified = true;
        }

        if (refreshTokenExpiresAt != default)
        {
            userSession.UserSessionRefreshTokenExpiration = refreshTokenExpiresAt;
            _dbContext.Entry(userSession).Property(s => s.UserSessionRefreshTokenExpiration).IsModified = true;
        }

        userSession.UpdateTime = DateTime.UtcNow;
        userSession.UpdateUser = updatedBy;

        _dbContext.Entry(userSession).Property(s => s.UpdateTime).IsModified = true;
        _dbContext.Entry(userSession).Property(s => s.UpdateUser).IsModified = true;

        return userSession;
    }

    /// <summary>
    /// Sets <c>user_session_is_revoked = true</c> for the given session.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public async Task RevokeAsync(UserSession userSession, string revokedBy)
    {
        _dbContext.Attach(userSession);

        userSession.UserSessionIsRevoked = true;
        userSession.UpdateTime = DateTime.UtcNow;
        userSession.UpdateUser = revokedBy;

        _dbContext.Entry(userSession).Property(s => s.UserSessionIsRevoked).IsModified = true;
        _dbContext.Entry(userSession).Property(s => s.UpdateTime).IsModified = true;
        _dbContext.Entry(userSession).Property(s => s.UpdateUser).IsModified = true;

    }

    /// <summary>
    /// Revokes all active sessions for a user at once.
    /// Called on password change or account deletion.
    /// Returns the number of rows affected.
    /// </summary>
    public async Task RevokeAllByUserIdAsync(User user, string revokedBy)
    {
        List<UserSession> sessionsToRevoke = await _dbContext.UserSessions
           .Where(s => s.UserId == user.UserId && s.UserSessionIsRevoked == false && !s.IsDeleted && s.UserSessionRefreshTokenExpiration > DateTime.UtcNow)
           .ToListAsync();

        foreach (UserSession session in sessionsToRevoke)
        {
            session.UserSessionIsRevoked = true;
            session.UpdateTime = DateTime.UtcNow;
            session.UpdateUser = revokedBy;

            _dbContext.Entry(session).Property(s => s.UserSessionIsRevoked).IsModified = true;
            _dbContext.Entry(session).Property(s => s.UpdateTime).IsModified = true;
            _dbContext.Entry(session).Property(s => s.UpdateUser).IsModified = true;
        }

    }

    /// <summary>
    /// Updates the push-notification token for the given session.
    /// Pass <c>null</c> in <paramref name="token"/> to clear it.
    /// </summary>
    public async Task SetPushNotificationTokenAsync(UserSession userSession, string? token, string updatedBy)
    {
        _dbContext.Attach(userSession);

        userSession.UserSessionPushNotificationToken = token;
        userSession.UpdateTime = DateTime.UtcNow;
        userSession.UpdateUser = updatedBy;

        _dbContext.Entry(userSession).Property(s => s.UserSessionPushNotificationToken).IsModified = true;
        _dbContext.Entry(userSession).Property(s => s.UpdateTime).IsModified = true;
        _dbContext.Entry(userSession).Property(s => s.UpdateUser).IsModified = true;

    }
}
