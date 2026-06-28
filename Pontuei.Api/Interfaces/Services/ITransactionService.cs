using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for transaction management.
/// Handles point estimation, status transitions, ownership enforcement,
/// and media coordination for the transaction lifecycle.
/// </summary>
public interface ITransactionService
{
    /// <summary>
    /// Returns a summary of the user's transactions for the dashboard view.
    /// Includes the top 3 programs with the most pending transactions and an "In Others"
    /// card aggregating all other programs.
    /// </summary>
    /// <param name="userId"></param>
    /// <returns></returns>
    Task<ApiResult<GetDashboardSummaryResponseDto>> GetDashboardSummaryAsync(Guid userId);

    /// <summary>
    /// Returns the full detail of a transaction, including media, for display
    /// on the "Detalhes de transação" screen.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<TransactionDetailResponseDto>> GetByIdAsync(Guid userId, Guid transactionId, Guid currentUserId);

    /// <summary>
    /// Returns all transactions for a given user, optionally filtered by status and/or loyalty program.
    /// Results are ordered by creation time descending, as shown on the "Minhas transações" screen.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    Task<ApiResult<GetTransactionsResponseDto>> GetAllAsync(Guid userId, GetTransactionsRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Registers a new transaction from the "Criar nova transação" screen.
    /// Calculates <c>transaction_estimated_points</c> from total value × points-per-real,
    /// validates that the loyalty program is active, and persists any attached media.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the referenced loyalty program is not found or inactive.</exception>
    Task<ApiResult<TransactionDetailResponseDto>> CreateAsync(Guid userId, CreateTransactionRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Applies partial edits to a pending transaction (pencil icon on the detail screen).
    /// Recalculates estimated points when value or points-per-real changes.
    /// Only pending transactions may be edited.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is not in Pending status.</exception>
    Task<ApiResult<TransactionDetailResponseDto>> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Transitions a transaction's status via the "Atualizar status" action sheet.
    /// Accepted transitions: Pending → Received, Pending → Disputed.
    /// Sets <c>transaction_actual_received_points</c> when marking as Received.
    /// Dispatches a notification to the user on success.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the requested status transition is not allowed.</exception>
    Task<ApiResult<TransactionDetailResponseDto>> UpdateStatusAsync(Guid userId, Guid transactionId, UpdateTransactionStatusRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Deletes a transaction (trash-can icon on the detail screen).
    /// Only the owning user may delete their own transactions.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<TransactionDetailResponseDto>> DeleteAsync(Guid userId, Guid transactionId, Guid currentUserId);

    // ── Media ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new media entry to an existing transaction.
    /// Validates ownership before persisting.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<TransactionMediaDto>> AddMediaAsync(Guid userId, Guid transactionId, UpsertTransactionMediaRequestDto dto, Guid currentUserId);

    /// <summary>
    /// Replaces all media entries for a transaction atomically (Salvar on Mídias screen).
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<IEnumerable<TransactionMediaDto>>> BulkReplaceMediaAsync(Guid userId, Guid transactionId, IEnumerable<UpsertTransactionMediaRequestDto> dtos, Guid currentUserId);

    /// <summary>
    /// Removes a single media entry from a transaction.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the media entry or transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    Task<ApiResult<TransactionMediaDto>> DeleteMediaAsync(Guid userId, Guid transactionId, Guid transactionMediaId, Guid currentUserId);
}
