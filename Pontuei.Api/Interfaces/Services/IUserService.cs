using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for user management.
/// Orchestrates repository calls, hashing, validation, and downstream side-effects
/// (e-mail dispatch, session invalidation, etc.).
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Returns a paginated list of users matching the given filter criteria.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    Task<ApiResult<GetUsersResponseDto>> GetUsers(GetUsersRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Returns a user summary by ID, or <c>null</c> when not found.
    /// </summary>
    Task<ApiResult<UserDto>> GetByIdAsync(Guid userId, Guid currentUserId);

    /// <summary>
    /// Registers a new user account from the "Nova conta" screen.
    /// Validates uniqueness, hashes the password, persists the user,
    /// and dispatches the e-mail verification code.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the e-mail is already registered.</exception>
    Task<ApiResult<UserDto>> RegisterAsync(CreateUserRequestDto dto, Guid? currentUserId);

    /// <summary>
    /// Applies partial updates from the "Informações de conta" settings screen.
    /// When <c>userEmail</c> changes, a new verification flow is triggered.
    /// Returns the updated summary.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the new e-mail is already taken.</exception>
    Task<ApiResult<UserDto>> UpdateAsync(Guid userId, UpdateUserRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Changes the authenticated user's password after verifying the current one.
    /// Invalidates all existing sessions on success.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when <c>currentPassword</c> does not match.</exception>
    Task<ApiResult<UserDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Soft-deletes the user account and revokes all active sessions.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    Task<ApiResult<EmptyDto>> DeleteAccountAsync(Guid userId, Guid currentUserId);
}
