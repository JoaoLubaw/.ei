using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>user_loyalty_program</c> table.
/// Manages the many-to-many enrollment between users and loyalty programs,
/// including per-user display ordering of the home-screen card carousel.
/// </summary>
public interface IUserLoyaltyProgramRepository
{
    /// <summary>
    /// Returns all enrollment records for the given user, ordered by
    /// <c>user_loyalty_program_display_order</c> ascending.
    /// Navigation property <c>LoyaltyProgram</c> is included.
    /// </summary>
    IQueryable<UserLoyaltyProgram> GetByUserIdAsync(Guid userId);

    /// <summary>
    /// Returns a single enrollment record, or <c>null</c> when the user
    /// is not enrolled in the given program.
    /// </summary>
    Task<UserLoyaltyProgram?> GetAsync(Guid userId, int loyaltyProgramId);

    /// <summary>
    /// Persists a new enrollment record and returns the saved entity.
    /// </summary>
    Task<UserLoyaltyProgram> CreateAsync(CreateUserLoyaltyProgramRequestDto dto, string createdBy);

    /// <summary>
    /// Atomically replaces the full enrollment list for a user with the provided set.
    /// Used by the bulk-save operation on the card-reorder screen ("Salvar").
    /// Deletes records not present in <paramref name="programs"/> and inserts/updates the rest.
    /// </summary>
    Task BulkReplaceAsync(Guid userId, IEnumerable<UserLoyaltyProgram> programs, string updatedBy);

    /// <summary>
    /// Removes a single enrollment record.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task<bool> DeleteAsync(Guid userId, int loyaltyProgramId, string deletedBy);
}
