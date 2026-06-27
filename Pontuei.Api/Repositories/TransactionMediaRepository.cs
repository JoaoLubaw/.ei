
using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Data;

using Pontuei.Api.Dtos.Requests;

using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class TransactionMediaRepository : BaseRepository, ITransactionMediaRepository
{
    public TransactionMediaRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the media entry with the given <paramref name="transactionMediaId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    public async Task<TransactionMedia?> GetByIdAsync(Guid transactionMediaId)
    {
        return await _dbContext.TransactionMedias.FirstOrDefaultAsync(tm => tm.TransactionMediaId == transactionMediaId);
    }

    /// <summary>
    /// Returns all media entries for the given transaction, ordered by
    /// <c>transaction_media_display_order</c> ascending.
    /// </summary>
    public IQueryable<TransactionMedia> GetByTransactionIdAsync(Guid transactionId)
    {
        return _dbContext.TransactionMedias.Where(tm => tm.TransactionId == transactionId).OrderBy(tm => tm.CreationTime);
    }

    /// <summary>
    /// Persists a new media entry and returns the saved entity.
    /// </summary>
    public async Task<TransactionMedia> CreateAsync(Transaction transaction, UpsertTransactionMediaRequestDto media, string createdBy)
    {
        TransactionMedia transactionMedia = new TransactionMedia
        {
            TransactionId = transaction.TransactionId,
            TransactionMediaFileType = media.TransactionMediaFileType,
            TransactionMediaFileUrl = media.TransactionMediaFileUrl,
            TransactionMediaDisplayOrder = media.TransactionMediaDisplayOrder,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.TransactionMedias.Add(transactionMedia);

        return transactionMedia;
    }

    public async Task BatchCreateAsync(Transaction transaction, IEnumerable<UpsertTransactionMediaRequestDto> medias, string createdBy)
    {
        List<TransactionMedia> transactionMedias = medias.Select(media => new TransactionMedia
        {
            TransactionId = transaction.TransactionId,
            TransactionMediaFileType = media.TransactionMediaFileType,
            TransactionMediaFileUrl = media.TransactionMediaFileUrl,
            TransactionMediaDisplayOrder = media.TransactionMediaDisplayOrder,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        }).ToList();

        _dbContext.TransactionMedias.AddRange(transactionMedias);
    }

    /// <summary>
    /// Replaces all media entries for a transaction atomically.
    /// Deletes existing rows not present in <paramref name="medias"/> and inserts the new ones.
    /// Used by the "Salvar" action on the Mídias edit screen.
    /// </summary>
    public async Task BulkReplaceAsync(Transaction transaction, IEnumerable<UpsertTransactionMediaRequestDto> medias, string updatedBy)
    {
        List<TransactionMedia> existingMedias = await _dbContext.TransactionMedias
            .Where(tm => tm.TransactionId == transaction.TransactionId).ToListAsync();

        List<TransactionMedia> mediasToDelete = existingMedias
            .Where(existing => !medias.Any(newMedia => newMedia.TransactionMediaFileUrl == existing.TransactionMediaFileUrl))
            .ToList();

        List<TransactionMedia> mediasToAdd = medias
            .Where(newMedia => !existingMedias.Any(existing => existing.TransactionMediaFileUrl == newMedia.TransactionMediaFileUrl))
            .Select(newMedia => new TransactionMedia
            {
                TransactionId = transaction.TransactionId,
                TransactionMediaFileType = newMedia.TransactionMediaFileType,
                TransactionMediaFileUrl = newMedia.TransactionMediaFileUrl,
                TransactionMediaDisplayOrder = newMedia.TransactionMediaDisplayOrder,
                CreationTime = DateTime.UtcNow,
                CreationUser = updatedBy
            }).ToList();

        _dbContext.TransactionMedias.RemoveRange(mediasToDelete);
        _dbContext.TransactionMedias.AddRange(mediasToAdd);
    }

    /// <summary>
    /// Deletes a single media entry by ID.
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public async Task DeleteAsync(TransactionMedia media, string deletedBy)
    {
        _dbContext.TransactionMedias.Remove(media);
    }

    /// <summary>
    /// Deletes all media entries for the given transaction.
    /// Returns the number of rows deleted.
    /// </summary>
    public async Task DeleteAllByTransactionAsync(Transaction transaction)
    {
        if (transaction.TransactionMedias.Any())
        {
            _dbContext.TransactionMedias.RemoveRange(transaction.TransactionMedias);
        }
    }
}
