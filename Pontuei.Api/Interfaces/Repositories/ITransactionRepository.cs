using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>transaction</c> table.
/// All queries are scoped to a specific user — no method returns transactions
/// from multiple users at once.
/// </summary>
public interface ITransactionRepository
{
    /// <summary>
    /// Returns the transaction with the given <paramref name="transactionId"/>,
    /// including the <c>LoyaltyProgram</c> and <c>TransactionMedias</c> navigation
    /// properties. Returns <c>null</c> when not found.
    /// </summary>
    Task<Transaction?> GetCompleteByIdAsync(Guid transactionId, bool verifyDeleted = true);

    /// <summary>
    /// Returns the transaction with the given <paramref name="transactionId"/>,
    /// or <c>null</c> when not found. Does not include navigation properties.
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="verifyDeleted"></param>
    /// <returns></returns>
    Task<Transaction?> GetByIdAsync(Guid transactionId, bool verifyDeleted = true);

    /// <summary>
    /// Returns all pending (status = Pending) transactions for the given user,
    /// including <c>LoyaltyProgram</c>. Ordered by <c>transaction_purchase_date</c> descending.
    /// These are the rows shown in the home-screen "Transações esperadas" list.
    /// </summary>
    IQueryable<Transaction> GetAllByUserIdAsync(Guid userId);

    /// <summary>
    /// Returns all pending transactions for the given user filtered to a specific
    /// loyalty program. Used when rendering the per-program card on the home screen.
    /// </summary>
    IQueryable<Transaction> GetPendingByUserAndProgramAsync(
        Guid userId,
        int loyaltyProgramId
    );

    /// <summary>
    /// Persists a new transaction (and its associated media, when provided via
    /// cascade) and returns the saved entity.
    /// </summary>
    Task<Transaction> CreateAsync(CreateTransactionRequestDto transactionDto, Guid userId, string createdBy);

    /// <summary>
    /// Applies field-level changes to an existing transaction row and returns the
    /// updated entity.
    /// </summary>
    Task<Transaction> UpdateAsync(Transaction transaction, UpdateTransactionRequestDto transactionDto, LoyaltyProgram? newLoyaltyProgram, string updatedBy);

    /// <summary>
    /// Deletes a transaction row and its associated media rows (cascade).
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    Task DeleteAsync(Transaction transaction, string deletedBy);
}
