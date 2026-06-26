using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for managing a user's personal loyalty program list.
/// Handles enrollment, removal, and the display-order reordering shown in the
/// home-screen carousel and the program-picker screens.
/// </summary>
public interface IUserLoyaltyProgramService
{
    /// <summary>
    /// Returns the authenticated user's enrolled programs ordered by display order.
    /// Each entry includes the loyalty program's branding details for card rendering.
    /// </summary>
    Task<ApiResult<GetUserLoyaltyProgramsResponseDto>> GetByUserIdAsync(Guid userId, GetUserLoyaltyProgramsRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Enrolls the user in a single loyalty program.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the loyalty program does not exist or is inactive.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the user is already enrolled.</exception>
    Task<ApiResult<UserLoyaltyProgramDto>> EnrollAsync(Guid userId, CreateUserLoyaltyProgramRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Replaces the user's full program list in a single atomic operation.
    /// Used when the user taps "Salvar" on the card-reorder / onboarding screen.
    /// Programs not present in the new list are removed.
    /// </summary>
    /// <exception cref="KeyNotFoundException">
    /// Thrown when any of the referenced program IDs do not exist or are inactive.
    /// </exception>
    Task<ApiResult<GetUserLoyaltyProgramsResponseDto>> BulkUpdateAsync(Guid userId, BulkUpdateUserLoyaltyProgramsRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Removes the user's enrollment in a single loyalty program.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the enrollment record is not found.</exception>
    Task<ApiResult<bool>> UnenrollAsync(Guid userId, int loyaltyProgramId, Guid currentUserId);
}
