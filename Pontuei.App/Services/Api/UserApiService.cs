using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// Covers all user-related API endpoints:
///
///   GET    /users/{userId}
///   PATCH  /users/{userId}
///   DELETE /users/{userId}
/// </summary>
public class UserApiService
{
    private readonly ApiClient _api;

    public UserApiService(ApiClient api)
    {
        _api = api;
    }

    /// <summary>
    /// Returns the details of a specific user by their ID.
    /// </summary>
    public Task<ApiResponse<UserDto>> GetUserByIdAsync(Guid userId)
        => _api.GetAsync<UserDto>($"users/{userId}");

    /// <summary>
    /// Partially updates a user's information (account settings screen).
    /// When the email is changed, a new verification flow is triggered by the API.
    /// </summary>
    public Task<ApiResponse<UserDto>> UpdateUserAsync(Guid userId, UpdateUserRequestDto request)
        => _api.PatchAsync<UserDto>($"users/{userId}", request);

    /// <summary>
    /// Account deletion endpoint. Permanently deletes the user and all their data (transactions, media, etc.).
    /// </summary>
    public Task<ApiResponse<EmptyDto>> DeleteAccountAsync(Guid userId)
        => _api.DeleteAsync<EmptyDto>($"users/{userId}");

}