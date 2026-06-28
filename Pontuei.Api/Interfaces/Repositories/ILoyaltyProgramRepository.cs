using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Requests;
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
    Task<LoyaltyProgram?> GetByIdAsync(int loyaltyProgramId, bool VerifyActive = false);

    /// <summary>
    /// Returns all loyalty programs, optionally filtered to active-only records.
    /// Results are ordered by name ascending.
    /// </summary>
    IQueryable<LoyaltyProgram> GetAllAsync();

    /// <summary>
    /// Returns the loyalty program with the given <paramref name="loyaltyProgramName"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    /// <param name="loyaltyProgramName"></param>
    /// <returns></returns>
    Task<LoyaltyProgram?> GetByNameAsync(string loyaltyProgramName);

    /// <summary>
    /// Persists a new loyalty program and returns the saved entity.
    /// </summary>
    Task<LoyaltyProgram> CreateAsync(CreateLoyaltyProgramRequestDto createDto, string createdBy);

    /// <summary>
    /// Applies changes to an existing loyalty program row and returns the updated entity.
    /// </summary>
    Task<LoyaltyProgram> UpdateAsync(LoyaltyProgram loyaltyProgram, UpdateLoyaltyProgramRequestDto updateDto, string updatedBy);

    /// <summary>
    /// Toggles the active status of the loyalty program.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task<bool> ToggleActiveAsync(LoyaltyProgram loyaltyProgram, string updatedBy);
}
