
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>transaction_media</c> table.
/// Manages receipt images and screenshots attached to transactions
/// (the "Mídias" section on the transaction detail screen).
/// </summary>
public interface ITransactionMediaRepository
{
    /// <summary>
    /// Returns the media entry with the given <paramref name="transactionMediaId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    Task<TransactionMedia?> GetByIdAsync(Guid transactionMediaId);

    /// <summary>
    /// Returns all media entries for the given transaction, ordered by
    /// <c>transaction_media_display_order</c> ascending.
    /// </summary>
    IQueryable<TransactionMedia> GetByTransactionIdAsync(Guid transactionId);

    /// <summary>
    /// Persists a new media entry and returns the saved entity.
    /// </summary>
    Task<TransactionMedia> CreateAsync(Transaction transaction, UpsertTransactionMediaRequestDto media, string createdBy);

    /// <summary>
    /// Creates all media entries for a transaction atomically.
    /// </summary>
    /// <param name="transaction"></param>
    /// <param name="medias"></param>
    /// <param name="createdBy"></param>
    /// <returns></returns>
    Task BatchCreateAsync(Transaction transaction, IEnumerable<UpsertTransactionMediaRequestDto> medias, string createdBy);

    /// <summary>
    /// Replaces all media entries for a transaction atomically.
    /// Deletes existing rows not present in <paramref name="medias"/> and inserts the new ones.
    /// Used by the "Salvar" action on the Mídias edit screen.
    /// </summary>
    Task BulkReplaceAsync(Transaction transaction, IEnumerable<UpsertTransactionMediaRequestDto> medias, string updatedBy);

    /// <summary>
    /// Deletes a single media entry by ID.
    /// </summary>
    Task DeleteAsync(TransactionMedia media, string deletedBy);

    /// <summary>
    /// Deletes all media entries for the given transaction.
    /// </summary>
    Task DeleteAllByTransactionAsync(Transaction transaction);
}
