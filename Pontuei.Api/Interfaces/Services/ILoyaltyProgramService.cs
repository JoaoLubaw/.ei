using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for managing the global loyalty program catalogue (admin operations).
/// </summary>
public interface ILoyaltyProgramService
{
    /// <summary>
    /// Returns the full loyalty program detail for a given ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the program does not exist.</exception>
    Task<ApiResult<LoyaltyProgramDto>> GetByIdAsync(int loyaltyProgramId, Guid currentUserId);

    /// <summary>
    /// Returns all loyalty programs available for user enrollment,
    /// ordered by name. Only active programs are returned to non-admin callers.
    /// </summary>
    Task<ApiResult<GetLoyaltyProgramsResponseDto>> GetAllAsync(GetLoyaltyProgramsRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Creates a new loyalty program in the catalogue.
    /// Validates name uniqueness before persisting.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a program with the same name already exists.</exception>
    Task<ApiResult<LoyaltyProgramDto>> CreateAsync(CreateLoyaltyProgramRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Applies partial updates to an existing loyalty program.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the program is not found.</exception>
    Task<ApiResult<LoyaltyProgramDto>> UpdateAsync(int loyaltyProgramId, UpdateLoyaltyProgramRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Soft-deletes a loyalty program from the catalogue.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the program is not found.</exception>
    Task<ApiResult<bool>> ToggleActiveAsync(int loyaltyProgramId, Guid currentUserId);
}
