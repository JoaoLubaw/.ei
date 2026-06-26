using Pontuei.Api.Dtos;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>loyalty_program</c> table.
/// Programs are global entities managed by admins and selected by users.
/// </summary>
public interface ILoyaltyProgramRepository
{
    /// <summary>
    /// Returns the loyalty program with the given <paramref name="loyaltyProgramId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    Task<LoyaltyProgram?> GetByIdAsync(int loyaltyProgramId);

    /// <summary>
    /// Returns all loyalty programs, optionally filtered to active-only records.
    /// Results are ordered by name ascending.
    /// </summary>
    IQueryable<LoyaltyProgram> GetAllAsync();

    /// <summary>
    /// Persists a new loyalty program and returns the saved entity.
    /// </summary>
    Task<LoyaltyProgram> CreateAsync(CreateLoyaltyProgramRequestDto createDto, string createdBy);

    /// <summary>
    /// Applies changes to an existing loyalty program row and returns the updated entity.
    /// </summary>
    Task<LoyaltyProgram> UpdateAsync(UpdateLoyaltyProgramRequestDto updateDto, string updatedBy);

    /// <summary>
    /// Soft-deletes the loyalty program by setting <c>row_is_deleted = true</c>.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task<bool> SoftDeleteAsync(int loyaltyProgramId, string deletedBy);
}
