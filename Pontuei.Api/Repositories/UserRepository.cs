using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Shared.Dtos.Requests;

using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class UserRepository : BaseRepository, IUserRepository
{
    public UserRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the user with the given <paramref name="userId"/>,
    /// or <c>null</c> if no record is found.
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid userId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserId == userId);
    }

    /// <summary>
    /// Returns the user whose <c>user_email</c> matches <paramref name="email"/> (case-insensitive),
    /// or <c>null</c> if no record is found.
    /// </summary>
    public async Task<User?> GetByEmailAsync(string email, bool verifyNotDeleted = false)
    {
        IQueryable<User> query = _dbContext.Users
            .Where(u => u.UserEmail.ToLower() == email.ToLower() && (!verifyNotDeleted || !u.IsDeleted));

        return await query.FirstOrDefaultAsync();
    }

    /// <summary>
    /// Returns the user whose <c>user_google_id</c> matches <paramref name="googleId"/>,
    /// or <c>null</c> if no record is found.
    /// </summary>
    public async Task<User?> GetByGoogleIdAsync(string googleId)
    {
        return await _dbContext.Users
            .FirstOrDefaultAsync(u => u.UserGoogleId == googleId);
    }

    /// <summary>
    /// Returns an <see cref="IQueryable{User}"/> representing all users in the database.
    /// </summary>
    /// <returns></returns>
    public IQueryable<User> GetAllUsers()
    {
        return _dbContext.Users;
    }

    /// <summary>
    /// Persists a new <see cref="User"/> row and returns the saved entity
    /// (with database-generated fields populated).
    /// </summary>
    public Task<User> CreateAsync(CreateUserRequestDto requestDto, string passwordHash, string createdBy)
    {
        User user = new User
        {
            UserEmail = requestDto.UserEmail,
            UserPasswordHash = passwordHash,
            UserName = requestDto.UserName,
            UserEmailNotificationsEnabled = requestDto.UserEmailNotificationsEnabled,
            UserPushNotificationsEnabled = requestDto.UserPushNotificationsEnabled,
            UserEmailVerified = false,
            UserIsAdmin = requestDto.UserIsAdmin ?? false,
            UserPhoneNumber = requestDto.UserPhoneNumber,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy,
            UserAcceptedTerms = requestDto.UserAcceptedTerms,
            UserAcceptedTermsAt = DateTime.UtcNow,
            UserAcceptedTermsVersion = "1.0.0"
        };

        _dbContext.Users.Add(user);

        return Task.FromResult(user);
    }

    /// <summary>
    /// Applies field-level changes to an existing user row and returns the updated entity.
    /// Callers are responsible for setting audit fields before invoking this method.
    /// </summary>
    public Task<User> UpdateAsync(User user, UpdateUserRequestDto requestDto, string updatedBy)
    {
        _dbContext.Attach(user);

        if (!string.IsNullOrEmpty(requestDto.UserName))
        {
            user.UserName = requestDto.UserName;
            _dbContext.Entry(user).Property(u => u.UserName).IsModified = true;
        }

        if (!string.IsNullOrEmpty(requestDto.UserPhoneNumber))
        {
            user.UserPhoneNumber = requestDto.UserPhoneNumber;
            _dbContext.Entry(user).Property(u => u.UserPhoneNumber).IsModified = true;
        }

        if (requestDto.UserEmailNotificationsEnabled.HasValue)
        {
            user.UserEmailNotificationsEnabled = requestDto.UserEmailNotificationsEnabled.Value;
            _dbContext.Entry(user).Property(u => u.UserEmailNotificationsEnabled).IsModified = true;
        }

        if (requestDto.UserPushNotificationsEnabled.HasValue)
        {
            user.UserPushNotificationsEnabled = requestDto.UserPushNotificationsEnabled.Value;
            _dbContext.Entry(user).Property(u => u.UserPushNotificationsEnabled).IsModified = true;
        }

        return Task.FromResult(user);
    }

    public Task<User> UpdatePasswordAsync(User user, string newPasswordHash, string updatedBy)
    {
        _dbContext.Attach(user);

        user.UserPasswordHash = newPasswordHash;
        user.UpdateTime = DateTime.UtcNow;
        user.UpdateUser = updatedBy;

        _dbContext.Entry(user).Property(u => u.UserPasswordHash).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateTime).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateUser).IsModified = true;

        return Task.FromResult(user);
    }

    /// <summary>
    /// Soft-deletes the user by setting <c>row_is_deleted = true</c>.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public Task SoftDeleteAsync(User user, string deletedBy)
    {
        _dbContext.Attach(user);

        user.IsDeleted = true;
        user.UpdateTime = DateTime.UtcNow;
        user.UpdateUser = deletedBy;

        _dbContext.Entry(user).Property(u => u.IsDeleted).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateTime).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateUser).IsModified = true;

        return Task.CompletedTask;
    }

    /// <summary>
    /// Marks the user's e-mail as verified, setting <c>user_email_verified = true</c>
    /// </summary>
    public Task SetEmailVerifiedAsync(User user, string verifiedBy)
    {
        _dbContext.Attach(user);

        user.UserEmailVerified = true;
        user.UserEmailVerifiedAt = DateTime.UtcNow;
        user.UpdateTime = DateTime.UtcNow;
        user.UpdateUser = verifiedBy;

        _dbContext.Entry(user).Property(u => u.UserEmailVerified).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UserEmailVerifiedAt).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateTime).IsModified = true;
        _dbContext.Entry(user).Property(u => u.UpdateUser).IsModified = true;

        return Task.CompletedTask;
    }
}