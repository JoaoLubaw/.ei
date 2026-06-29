using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// Global API service for loyalty programs.:
///   GET    /loyalty-programs
///   GET    /loyalty-programs/{id}
///
/// User-specific programs:
///   GET    /users/{userId}/loyalty-programs
///   POST   /users/{userId}/loyalty-programs
///   PUT    /users/{userId}/loyalty-programs/bulk
///   DELETE /users/{userId}/loyalty-programs/{loyaltyProgramId}
/// </summary>
public class LoyaltyProgramsApiService
{
    private readonly ApiClient _api;

    public LoyaltyProgramsApiService(ApiClient api)
    {
        _api = api;
    }

    // ── Global Catalog ───────────────────────────────────────────────────

    /// <summary>
    /// Lists all loyalty programs in the catalog, with optional filtering and pagination.
    /// </summary>
    public Task<ApiResponse<GetLoyaltyProgramsResponseDto>> GetAllProgramsAsync(
        GetLoyaltyProgramsRequestDto? request = null)
    {
        string url = ApiClient.BuildQueryString("loyalty-programs", request);
        return _api.GetAsync<GetLoyaltyProgramsResponseDto>(url);
    }

    /// <summary>
    /// Details of a single loyalty program by its ID.
    /// </summary>
    public Task<ApiResponse<LoyaltyProgramDto>> GetProgramByIdAsync(int loyaltyProgramId)
        => _api.GetAsync<LoyaltyProgramDto>($"loyalty-programs/{loyaltyProgramId}");


    // ── User programs ─────────────────────────────────────────────────────

    /// <summary>
    /// Lists all loyalty programs that a specific user is enrolled in, with optional filtering and pagination.
    /// </summary>
    public Task<ApiResponse<GetUserLoyaltyProgramsResponseDto>> GetUserProgramsAsync(
        Guid userId,
        GetUserLoyaltyProgramsRequestDto? request = null)
    {
        string url = ApiClient.BuildQueryString($"users/{userId}/loyalty-programs", request);
        return _api.GetAsync<GetUserLoyaltyProgramsResponseDto>(url);
    }

    /// <summary>
    /// Subscribes the user to a single loyalty program.
    /// </summary>
    public Task<ApiResponse<UserLoyaltyProgramDto>> EnrollAsync(
        Guid userId,
        CreateUserLoyaltyProgramRequestDto request)
        => _api.PostAsync<UserLoyaltyProgramDto>($"users/{userId}/loyalty-programs", request);

    /// <summary>
    /// Replaces the entire list of programs for a user at once (reordering and onboarding screens).
    /// Programs not included in the request will be unenrolled, and new programs will be enrolled.
    /// </summary>
    public Task<ApiResponse<GetUserLoyaltyProgramsResponseDto>> BulkUpdateUserProgramsAsync(
        Guid userId,
        BulkUpdateUserLoyaltyProgramsRequestDto request)
        => _api.PutAsync<GetUserLoyaltyProgramsResponseDto>($"users/{userId}/loyalty-programs/bulk", request);

    /// <summary>
    /// Removes the user's enrollment from a specific loyalty program.
    /// </summary>
    public Task<ApiResponse<bool>> UnenrollAsync(Guid userId, int loyaltyProgramId)
        => _api.DeleteAsync<bool>($"users/{userId}/loyalty-programs/{loyaltyProgramId}");

}