using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>user</c> table.
/// Implementations must not contain business logic — only raw DB operations.
/// </summary>
public interface IUserRepository
{
    /// <summary>
    /// Returns the user with the given <paramref name="userId"/>,
    /// or <c>null</c> if no record is found.
    /// </summary>
    Task<User?> GetByIdAsync(Guid userId);

    /// <summary>
    /// Returns the user whose <c>user_email</c> matches <paramref name="email"/> (case-insensitive),
    /// or <c>null</c> if no record is found.
    /// </summary>
    Task<User?> GetByEmailAsync(string email);

    /// <summary>
    /// Returns the user whose <c>user_google_id</c> matches <paramref name="googleId"/>,
    /// or <c>null</c> if no record is found.
    /// </summary>
    Task<User?> GetByGoogleIdAsync(string googleId);

    /// <summary>
    /// Returns an <see cref="IQueryable{User}"/> representing all users in the database.
    /// </summary>
    /// <returns></returns>
    IQueryable<User> GetAllUsers();

    /// <summary>
    /// Persists a new <see cref="User"/> row and returns the saved entity
    /// (with database-generated fields populated).
    /// </summary>
    Task<User> CreateAsync(CreateUserRequestDto requestDto, string passwordHash, string createdBy);

    /// <summary>
    /// Applies field-level changes to an existing user row and returns the updated entity.
    /// Callers are responsible for setting audit fields before invoking this method.
    /// </summary>
    Task<User> UpdateAsync(User user, UpdateUserRequestDto requestDto, string updatedBy);

    /// <summary>
    /// Soft-deletes the user by setting <c>row_is_deleted = true</c>.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task SoftDeleteAsync(User user, string deletedBy);

    /// <summary>
    /// Marks the user's e-mail as verified, setting <c>user_email_verified = true</c>
    /// </summary>
    Task SetEmailVerifiedAsync(User user, string verifiedBy);
}