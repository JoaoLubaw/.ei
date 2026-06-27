using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class TransactionRepository : BaseRepository, ITransactionRepository
{
    public TransactionRepository(PontueiDbContext dbContext) : base(dbContext)
    {

    }

    /// <summary>
    /// Returns the transaction with the given <paramref name="transactionId"/>,
    /// including the <c>LoyaltyProgram</c> and <c>TransactionMedias</c> navigation
    /// properties. Returns <c>null</c> when not found.
    /// </summary>
    public async Task<Transaction?> GetCompleteByIdAsync(Guid transactionId, bool verifyDeleted = true)
    {
        return await _dbContext.Transactions
            .Include(t => t.LoyaltyProgram)
            .Include(t => t.TransactionMedias)
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId && (!verifyDeleted || !t.IsDeleted));
    }

    /// <summary>
    /// Returns the transaction with the given <paramref name="transactionId"/>,
    /// or <c>null</c> when not found. Does not include navigation properties.
    /// </summary>
    /// <param name="transactionId"></param>
    /// <param name="verifyDeleted"></param>
    /// <returns></returns>
    public async Task<Transaction?> GetByIdAsync(Guid transactionId, bool verifyDeleted = true)
    {
        return await _dbContext.Transactions
            .FirstOrDefaultAsync(t => t.TransactionId == transactionId && (!verifyDeleted || !t.IsDeleted));
    }

    /// <summary>
    /// Returns all transactions for the given user,
    /// including <c>LoyaltyProgram</c>. Ordered by <c>transaction_purchase_date</c> descending.
    /// These are the rows shown in the home-screen "Transações esperadas" list.
    /// </summary>
    public IQueryable<Transaction> GetAllByUserIdAsync(Guid userId)
    {
        return _dbContext.Transactions.Where(t => t.UserId == userId && !t.IsDeleted).OrderByDescending(t => t.TransactionPurchaseDate);
    }

    /// <summary>
    /// Returns all pending transactions for the given user filtered to a specific
    /// loyalty program. Used when rendering the per-program card on the home screen.
    /// </summary>
    public IQueryable<Transaction> GetPendingByUserAndProgramAsync(Guid userId, int loyaltyProgramId)
    {
        return _dbContext.Transactions
            .Where(t => t.UserId == userId && t.LoyaltyProgramId == loyaltyProgramId && !t.IsDeleted)
            .OrderByDescending(t => t.TransactionPurchaseDate);
    }

    /// <summary>
    /// Persists a new transaction (and its associated media, when provided via
    /// cascade) and returns the saved entity.
    /// </summary>
    public async Task<Transaction> CreateAsync(CreateTransactionRequestDto transactionDto, Guid userId, string createdBy)
    {
        TransactionStatus status = DetermineTransactionStatus(transactionDto.TransactionPurchaseDate, transactionDto.TransactionItemReceiptDate, transactionDto.TransactionReceiptDeadlineDays);

        Transaction transaction = new Transaction
        {
            UserId = userId,
            LoyaltyProgramId = transactionDto.LoyaltyProgramId,
            TransactionPurchaseDate = transactionDto.TransactionPurchaseDate,
            TransactionStatus = status,
            TransactionTotalValue = transactionDto.TransactionTotalValue,
            TransactionDescription = transactionDto.TransactionDescription,
            TransactionStore = transactionDto.TransactionStore,
            TransactionEstimatedPoints = transactionDto.TransactionTotalValue * transactionDto.TransactionPointsPerReal,
            TransactionItemReceiptDate = transactionDto.TransactionItemReceiptDate,
            TransactionPointsPerReal = transactionDto.TransactionPointsPerReal,
            TransactionReceiptDeadlineDays = transactionDto.TransactionReceiptDeadlineDays,
            CreationTime = DateTime.UtcNow,
            CreationUser = createdBy
        };

        _dbContext.Transactions.Add(transaction);
        await _dbContext.SaveChangesAsync();

        return transaction;
    }

    /// <summary>
    /// Applies field-level changes to an existing transaction row and returns the
    /// updated entity.
    /// </summary>
    public async Task<Transaction> UpdateAsync(
            Transaction transaction,
            UpdateTransactionRequestDto transactionDto,
            LoyaltyProgram? newLoyaltyProgram,
            string updatedBy
        )
    {
        _dbContext.Attach(transaction);

        if (transactionDto.TransactionPurchaseDate.HasValue && transactionDto.TransactionPurchaseDate.Value != transaction.TransactionPurchaseDate)
        {
            transaction.TransactionPurchaseDate = transactionDto.TransactionPurchaseDate.Value;
            _dbContext.Entry(transaction).Property(t => t.TransactionPurchaseDate).IsModified = true;
        }

        if (transactionDto.TransactionTotalValue.HasValue && transactionDto.TransactionTotalValue.Value != transaction.TransactionTotalValue)
        {
            transaction.TransactionTotalValue = transactionDto.TransactionTotalValue.Value;
            _dbContext.Entry(transaction).Property(t => t.TransactionTotalValue).IsModified = true;
        }

        if (transactionDto.TransactionDescription != null && transactionDto.TransactionDescription != transaction.TransactionDescription)
        {
            transaction.TransactionDescription = transactionDto.TransactionDescription;
            _dbContext.Entry(transaction).Property(t => t.TransactionDescription).IsModified = true;
        }

        if (transactionDto.TransactionStore != null && transactionDto.TransactionStore != transaction.TransactionStore)
        {
            transaction.TransactionStore = transactionDto.TransactionStore;
            _dbContext.Entry(transaction).Property(t => t.TransactionStore).IsModified = true;
        }

        if (transactionDto.TransactionItemReceiptDate.HasValue && transactionDto.TransactionItemReceiptDate.Value != transaction.TransactionItemReceiptDate)
        {
            transaction.TransactionItemReceiptDate = transactionDto.TransactionItemReceiptDate;
            _dbContext.Entry(transaction).Property(t => t.TransactionItemReceiptDate).IsModified = true;
        }

        if (transactionDto.TransactionPointsPerReal.HasValue && transactionDto.TransactionPointsPerReal.Value != transaction.TransactionPointsPerReal)
        {
            transaction.TransactionPointsPerReal = transactionDto.TransactionPointsPerReal.Value;
            _dbContext.Entry(transaction).Property(t => t.TransactionPointsPerReal).IsModified = true;
        }

        if (transactionDto.TransactionReceiptDeadlineDays.HasValue && transactionDto.TransactionReceiptDeadlineDays.Value != transaction.TransactionReceiptDeadlineDays)
        {
            transaction.TransactionReceiptDeadlineDays = transactionDto.TransactionReceiptDeadlineDays.Value;
            _dbContext.Entry(transaction).Property(t => t.TransactionReceiptDeadlineDays).IsModified = true;
        }

        if (newLoyaltyProgram != null && newLoyaltyProgram.LoyaltyProgramId != transaction.LoyaltyProgramId)
        {
            transaction.LoyaltyProgramId = newLoyaltyProgram.LoyaltyProgramId;
            _dbContext.Entry(transaction).Property(t => t.LoyaltyProgramId).IsModified = true;
        }

        if (transactionDto.TransactionPointsPerReal.HasValue && transactionDto.TransactionTotalValue.HasValue)
        {
            decimal newEstimatedPoints = transactionDto.TransactionTotalValue.Value * transactionDto.TransactionPointsPerReal.Value;

            if (newEstimatedPoints != transaction.TransactionEstimatedPoints)
            {
                transaction.TransactionEstimatedPoints = newEstimatedPoints;
                _dbContext.Entry(transaction).Property(t => t.TransactionEstimatedPoints).IsModified = true;
            }
        }

        TransactionStatus newStatus = DetermineTransactionStatus(transaction.TransactionPurchaseDate, transaction.TransactionItemReceiptDate, transaction.TransactionReceiptDeadlineDays);
        if (newStatus != transaction.TransactionStatus)
        {
            transaction.TransactionStatus = newStatus;
            _dbContext.Entry(transaction).Property(t => t.TransactionStatus).IsModified = true;
        }

        transaction.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(transaction).Property(t => t.UpdateTime).IsModified = true;

        transaction.UpdateUser = updatedBy;
        _dbContext.Entry(transaction).Property(t => t.UpdateUser).IsModified = true;

        await _dbContext.SaveChangesAsync();

        return transaction;
    }

    /// <summary>
    /// Deletes a transaction row and its associated media rows (cascade).
    /// Returns <c>false</c> when no matching row is found.
    /// </summary>
    public async Task DeleteAsync(Transaction transaction, string deletedBy)
    {
        _dbContext.Attach(transaction);

        transaction.IsDeleted = true;
        _dbContext.Entry(transaction).Property(t => t.IsDeleted).IsModified = true;

        transaction.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(transaction).Property(t => t.UpdateTime).IsModified = true;

        transaction.UpdateUser = deletedBy;
        _dbContext.Entry(transaction).Property(t => t.UpdateUser).IsModified = true;

        foreach (TransactionMedia media in transaction.TransactionMedias)
        {
            media.IsDeleted = true;
            _dbContext.Entry(media).Property(tm => tm.IsDeleted).IsModified = true;

            media.UpdateTime = DateTime.UtcNow;
            _dbContext.Entry(media).Property(tm => tm.UpdateTime).IsModified = true;

            media.UpdateUser = deletedBy;
            _dbContext.Entry(media).Property(tm => tm.UpdateUser).IsModified = true;
        }
    }

    private TransactionStatus DetermineTransactionStatus(DateOnly purchaseDate, DateOnly? itemReceiptDate, int receiptDeadlineDays)
    {
        DateOnly? deadlineDate = null;

        if (itemReceiptDate.HasValue)
        {
            deadlineDate = itemReceiptDate.Value.AddDays(receiptDeadlineDays);
        }

        if (deadlineDate.HasValue)
        {
            string timeZoneId = OperatingSystem.IsWindows() ? "E. South America Standard Time" : "America/Sao_Paulo";
            TimeZoneInfo brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById(timeZoneId);

            DateTime localNow = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);

            DateOnly todayLocal = DateOnly.FromDateTime(localNow);

            if (todayLocal > deadlineDate)
            {
                return TransactionStatus.Late;
            }
        }

        return TransactionStatus.Pending;
    }
}
