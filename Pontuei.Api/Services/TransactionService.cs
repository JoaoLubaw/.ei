using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

public class TransactionService : ITransactionService
{
    private readonly ITransactionRepository _transactionRepository;
    private readonly IUserLoyaltyProgramRepository _userLoyaltyProgramRepository;
    private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
    private readonly ITransactionMediaRepository _transactionMediaRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TransactionService> _logger;

    public TransactionService(
        ITransactionRepository transactionRepository,
        IUserLoyaltyProgramRepository userLoyaltyProgramRepository,
        ILoyaltyProgramRepository loyaltyProgramRepository,
        ITransactionMediaRepository transactionMediaRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<TransactionService> logger)
    {
        _transactionRepository = transactionRepository;
        _userLoyaltyProgramRepository = userLoyaltyProgramRepository;
        _loyaltyProgramRepository = loyaltyProgramRepository;
        _transactionMediaRepository = transactionMediaRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResult<GetDashboardSummaryResponseDto>> GetDashboardSummaryAsync(Guid userId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(userId);

        if (loggedUser == null)
        {
            return new ApiResult<GetDashboardSummaryResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        List<LoyaltyProgram> userFavoritePrograms = await _userLoyaltyProgramRepository
                    .GetByUserIdAsync(userId)
                    .Where(ulp => ulp.LoyaltyProgram != null)
                    .Select(ulp => ulp.LoyaltyProgram!)
                    .ToListAsync();

        List<LoyaltyProgram> top3Programs = userFavoritePrograms.Take(3).ToList();

        HashSet<int> top3ProgramIds = top3Programs.Select(p => p.LoyaltyProgramId).ToHashSet();

        List<Transaction> pendingTransactions = await _transactionRepository.GetAllPendingByUserIdAsync(userId);

        GetDashboardSummaryResponseDto response = new GetDashboardSummaryResponseDto();

        if (top3Programs.Count == 0)
        {
            response.Others.TotalPendingPoints = pendingTransactions.Sum(t => t.TransactionEstimatedPoints);
            response.Others.PendingTransactions = pendingTransactions.Adapt<List<TransactionDto>>();

            return new ApiResult<GetDashboardSummaryResponseDto>(
                InternalResultCode.NO_ERROR,
                HttpStatusCode.OK,
                response
            );
        }

        foreach (LoyaltyProgram program in top3Programs)
        {
            List<Transaction> programTransactions = pendingTransactions
                .Where(t => t.LoyaltyProgramId == program.LoyaltyProgramId)
                .ToList();

            response.TopPrograms.Add(new DashboardProgramCardDto
            {
                LoyaltyProgram = program.Adapt<LoyaltyProgramDto>(),

                TotalPendingPoints = programTransactions.Sum(t => t.TransactionEstimatedPoints),

                PendingTransactions = programTransactions.Adapt<List<TransactionDto>>()
            });
        }

        List<Transaction> otherTransactions = pendingTransactions
            .Where(t => !top3ProgramIds.Contains(t.LoyaltyProgramId))
            .ToList();

        response.Others = new DashboardOthersCardDto
        {
            TotalPendingPoints = otherTransactions.Sum(t => t.TransactionEstimatedPoints),
            PendingTransactions = otherTransactions.Adapt<List<TransactionDto>>()
        };

        return new ApiResult<GetDashboardSummaryResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            response
        );
    }

    /// <summary>
    /// Returns the full detail of a transaction, including media, for display
    /// on the "Detalhes de transação" screen.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<TransactionDetailResponseDto>> GetByIdAsync(Guid userId, Guid transactionId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to access transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_GET_THIS_USER,
                HttpStatusCode.Forbidden,
                null);

        }

        Transaction? transaction = await _transactionRepository.GetCompleteByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        return new ApiResult<TransactionDetailResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            transaction.Adapt<TransactionDetailResponseDto>()
        );
    }

    /// <summary>
    /// Returns all transactions for a given user, optionally filtered by status and/or loyalty program.
    /// Results are ordered by creation time descending, as shown on the "Minhas transações" screen.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="dto"></param>
    /// <returns></returns>
    public async Task<ApiResult<GetTransactionsResponseDto>> GetAllAsync(Guid userId, GetTransactionsRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<GetTransactionsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to access transactions for user {UserId} without permission.", currentUserId, userId);

            return new ApiResult<GetTransactionsResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_GET_THIS_USER,
                    HttpStatusCode.Forbidden,
                    null);
        }

        IQueryable<Transaction> query = _transactionRepository.GetAllByUserIdAsync(userId);
        ApplyFilters(query, dto);

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<Transaction> transactions = await query
            .Skip(skip)
            .Take(dto.Size)
            .ToListAsync();

        if (dto.Filters?.IsOverdue == true)
        {
            transactions = transactions.Where(t => t.IsOverdue).ToList();

            totalElements = transactions.Count;
            totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        }

        return new ApiResult<GetTransactionsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetTransactionsResponseDto
            {
                Page = dto.Page,
                Size = dto.Size,
                TotalElements = totalElements,
                TotalPages = totalPages,
                Transactions = transactions.Adapt<List<TransactionDto>>()
            }
        );
    }

    IQueryable<Transaction> ApplyFilters(IQueryable<Transaction> query, GetTransactionsRequestDto dto)
    {
        if (dto.Filters == null)
        {
            return query;
        }

        if (dto.Filters.TransactionId.HasValue)
            query = query.Where(t => t.TransactionId == dto.Filters.TransactionId.Value);

        if (dto.Filters.LoyaltyProgramId.HasValue)
            query = query.Where(t => t.LoyaltyProgramId == dto.Filters.LoyaltyProgramId.Value);

        if (dto.Filters.TransactionStatus.HasValue)
            query = query.Where(t => t.TransactionStatus == dto.Filters.TransactionStatus.Value);

        if (dto.Filters.TransactionPurchaseDate.HasValue)
            query = query.Where(t => t.TransactionPurchaseDate == dto.Filters.TransactionPurchaseDate.Value);

        if (!string.IsNullOrWhiteSpace(dto.Filters.TransactionStore))
            query = query.Where(t => EF.Functions.ILike(t.TransactionStore, "%" + dto.Filters.TransactionStore + "%"));

        return query;
    }

    /// <summary>
    /// Registers a new transaction from the "Criar nova transação" screen.
    /// Calculates <c>transaction_estimated_points</c> from total value × points-per-real,
    /// validates that the loyalty program is active, and persists any attached media.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the referenced loyalty program is not found or inactive.</exception>
    public async Task<ApiResult<TransactionDetailResponseDto>> CreateAsync(Guid userId, CreateTransactionRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to create a transaction for user {UserId} without permission.", currentUserId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                    HttpStatusCode.Forbidden,
                    null);
        }

        if (!dto.IsValid())
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.MISSING_INFORMATION,
                HttpStatusCode.BadRequest,
                null
            );
        }

        LoyaltyProgram? program = await _loyaltyProgramRepository.GetByIdAsync(dto.LoyaltyProgramId, VerifyActive: true);

        if (program == null)
        {
            _logger.LogWarning("Attempt to create transaction with inactive or non-existent loyalty program: {ProgramId}", dto.LoyaltyProgramId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        Transaction newTransaction = await _transactionRepository.CreateAsync(dto, userId, currentUserId.ToString());

        if (dto.Medias != null && dto.Medias.Any())
        {
            _logger.LogInformation("Creating {MediaCount} media items for transaction {TransactionId}", dto.Medias.Count, newTransaction.TransactionId);

            await _transactionMediaRepository.BatchCreateAsync(newTransaction, dto.Medias, currentUserId.ToString());
        }

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to save the transaction for user: {UserId}", userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null);
        }

        newTransaction.LoyaltyProgram = program;

        _logger.LogInformation("Successfully created transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", newTransaction.TransactionId, userId, currentUserId);

        return new ApiResult<TransactionDetailResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.Created,
            newTransaction.Adapt<TransactionDetailResponseDto>()
        );
    }


    /// <summary>
    /// Applies partial edits to a pending transaction (pencil icon on the detail screen).
    /// Recalculates estimated points when value or points-per-real changes.
    /// Only pending transactions may be edited.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is not in Pending status.</exception>
    public async Task<ApiResult<TransactionDetailResponseDto>> UpdateAsync(Guid userId, Guid transactionId, UpdateTransactionRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to update transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        Transaction? transaction = await _transactionRepository.GetCompleteByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (transaction.TransactionStatus != TransactionStatus.Pending
            && transaction.TransactionStatus != TransactionStatus.Late
            && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to update transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.TRANSACTION_NOT_PENDING,
                HttpStatusCode.BadRequest,
                null
            );
        }

        LoyaltyProgram? newProgram = null;
        if (dto.LoyaltyProgramId.HasValue && dto.LoyaltyProgramId.Value != transaction.LoyaltyProgramId)
        {
            _logger.LogInformation("User {CurrentUserId} is attempting to change the loyalty program for transaction {TransactionId} from {OldProgramId} to {NewProgramId}.", currentUserId, transactionId, transaction.LoyaltyProgramId, dto.LoyaltyProgramId.Value);

            newProgram = await _loyaltyProgramRepository.GetByIdAsync(dto.LoyaltyProgramId.Value, VerifyActive: true);

            if (newProgram == null)
            {
                return new ApiResult<TransactionDetailResponseDto>(
                    InternalResultCode.ENTITY_NOT_FOUND,
                    HttpStatusCode.NotFound,
                    null
                );
            }
        }

        _logger.LogInformation("Updating transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", transactionId, userId, currentUserId);

        Transaction updatedTransaction = await _transactionRepository.UpdateAsync(transaction, dto, newProgram, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to update transaction: {TransactionId}", transactionId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );
        }

        _logger.LogInformation("Successfully updated transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", transactionId, userId, currentUserId);

        return new ApiResult<TransactionDetailResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            updatedTransaction.Adapt<TransactionDetailResponseDto>()
        );
    }

    /// <summary>
    /// Transitions a transaction's status via the "Atualizar status" action sheet.
    /// Accepted transitions: Pending → Received, Pending → Disputed.
    /// Sets <c>transaction_actual_received_points</c> when marking as Received.
    /// Dispatches a notification to the user on success.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the requested status transition is not allowed.</exception>
    public async Task<ApiResult<TransactionDetailResponseDto>> UpdateStatusAsync(Guid userId, Guid transactionId, UpdateTransactionStatusRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to update status of transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        Transaction? transaction = await _transactionRepository.GetCompleteByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (transaction.TransactionStatus != TransactionStatus.Pending
        && transaction.TransactionStatus != TransactionStatus.Late
        && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to update status of transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.TRANSACTION_NOT_PENDING,
                HttpStatusCode.BadRequest,
                null
            );
        }

        _logger.LogInformation("Updating status of transaction {TransactionId} for user {UserId} to {NewStatus} by user {CurrentUserId}.", transactionId, userId, dto.TransactionStatus, currentUserId);

        transaction = await _transactionRepository.UpdateStatusAsync(transaction, dto.TransactionStatus, dto.TransactionActualReceivedPoints, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );

        _logger.LogInformation("Successfully updated status of transaction {TransactionId} for user {UserId} to {NewStatus}.", transactionId, userId, dto.TransactionStatus);

        return new ApiResult<TransactionDetailResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            transaction.Adapt<TransactionDetailResponseDto>()
        );
    }

    /// <summary>
    /// Deletes a transaction (trash-can icon on the detail screen).
    /// Only the owning user may delete their own transactions.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<TransactionDetailResponseDto>> DeleteAsync(Guid userId, Guid transactionId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to delete transaction {TransactionId} for user {UserId} without permission.", currentUserId, transactionId, userId);

            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        Transaction? transaction = await _transactionRepository.GetCompleteByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
        {
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        await _transactionRepository.DeleteAsync(transaction, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
            return new ApiResult<TransactionDetailResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );

        _logger.LogInformation("Successfully deleted transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", transactionId, userId, currentUserId);

        return new ApiResult<TransactionDetailResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            transaction.Adapt<TransactionDetailResponseDto>()
        );
    }

    // ── Media ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Adds a new media entry to an existing transaction.
    /// Validates ownership before persisting.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<TransactionMediaDto>> AddMediaAsync(Guid userId, Guid transactionId, UpsertTransactionMediaRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        Transaction? transaction = await _transactionRepository.GetByIdAsync(transactionId, verifyDeleted: true);

        if (transaction == null || transaction.UserId != userId)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );

        TransactionMedia newMedia = await _transactionMediaRepository.CreateAsync(transaction, dto, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );

        _logger.LogInformation("Successfully added media {MediaId} to transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", newMedia.TransactionMediaId, transactionId, userId, currentUserId);

        return new ApiResult<TransactionMediaDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.Created,
            newMedia.Adapt<TransactionMediaDto>()
        );
    }

    /// <summary>
    /// Replaces all media entries for a transaction atomically (Salvar on Mídias screen).
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<IEnumerable<TransactionMediaDto>>> BulkReplaceMediaAsync(Guid userId, Guid transactionId, IEnumerable<UpsertTransactionMediaRequestDto> dtos, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
            return new ApiResult<IEnumerable<TransactionMediaDto>>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );

        if (userId != currentUserId)
            return new ApiResult<IEnumerable<TransactionMediaDto>>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );

        Transaction? transaction = await _transactionRepository.GetByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
            return new ApiResult<IEnumerable<TransactionMediaDto>>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );

        await _transactionMediaRepository.BulkReplaceAsync(transaction, dtos, currentUserId.ToString());
        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
            return new ApiResult<IEnumerable<TransactionMediaDto>>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );

        IEnumerable<TransactionMedia> savedMedias = await _transactionMediaRepository.GetByTransactionIdAsync(transactionId).ToListAsync();

        _logger.LogInformation("Successfully replaced media for transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", transactionId, userId, currentUserId);

        return new ApiResult<IEnumerable<TransactionMediaDto>>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            savedMedias.Adapt<IEnumerable<TransactionMediaDto>>()
        );
    }

    /// <summary>
    /// Removes a single media entry from a transaction.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the media entry or transaction is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when the transaction does not belong to <paramref name="userId"/>.</exception>
    public async Task<ApiResult<TransactionMediaDto>> DeleteMediaAsync(Guid userId, Guid transactionId, Guid transactionMediaId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );

        if (userId != currentUserId)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );

        Transaction? transaction = await _transactionRepository.GetByIdAsync(transactionId);

        if (transaction == null || transaction.UserId != userId)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );

        TransactionMedia? media = await _transactionMediaRepository.GetByIdAsync(transactionMediaId);

        if (media == null || media.TransactionId != transactionId)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );

        await _transactionMediaRepository.DeleteAsync(media, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
            return new ApiResult<TransactionMediaDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );

        _logger.LogInformation("Successfully deleted media {MediaId} from transaction {TransactionId} for user {UserId} by user {CurrentUserId}.", transactionMediaId, transactionId, userId, currentUserId);

        return new ApiResult<TransactionMediaDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }
}
