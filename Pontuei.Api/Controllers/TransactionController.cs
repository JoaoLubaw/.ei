using System.Net;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Pontuei.Api.Commons;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;
using Pontuei.Api.Dtos;
using Pontuei.Api.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Pontuei.Api.Controllers;

[Route("users/{userId:guid}")]
[SwaggerTag("Transaction management and dashboard")]
[Authorize]
public class TransactionController : PontueiControllerBase
{
    private readonly ITransactionService _transactionService;
    private readonly IStorageService _storageService;

    public TransactionController(
        ITransactionService transactionService,
        IStorageService storageService,
        ILogger<TransactionController> logger) : base(logger)
    {
        _transactionService = transactionService;
        _storageService = storageService;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────

    /// <summary>
    /// Returns a summary of the user's transactions for the dashboard view.
    /// </summary>
    /// <remarks>
    /// Includes the top 3 programs with the most pending transactions and an "In Others"
    /// aggregation card for all remaining programs.
    /// </remarks>
    /// <response code="200">Returns the dashboard summary.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetDashboardSummaryResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet("dashboard")]
    public async Task<ActionResult<GetDashboardSummaryResponseDto>> GetDashboardSummary([FromRoute] Guid userId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<GetDashboardSummaryResponseDto> apiResult = await _transactionService.GetDashboardSummaryAsync(userId);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetDashboardSummary));
        }
    }

    // ── Transactions ──────────────────────────────────────────────────────

    /// <summary>
    /// Returns all transactions for a given user, optionally filtered by status and/or loyalty program.
    /// </summary>
    /// <remarks>
    /// Results are ordered by creation time descending, as shown on the "Minhas transações" screen.
    /// </remarks>
    /// <response code="200">Returns the transaction list.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(GetTransactionsResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpGet("transactions")]
    public async Task<ActionResult<GetTransactionsResponseDto>> GetTransactions(
        [FromRoute] Guid userId,
        [FromQuery] GetTransactionsRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<GetTransactionsResponseDto> apiResult = await _transactionService.GetAllAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetTransactions));
        }
    }

    /// <summary>
    /// Returns the full detail of a transaction, including media.
    /// </summary>
    /// <response code="200">Returns the transaction detail.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionDetailResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpGet("transactions/{transactionId:guid}")]
    public async Task<ActionResult<TransactionDetailResponseDto>> GetTransactionById(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<TransactionDetailResponseDto> apiResult = await _transactionService.GetByIdAsync(userId, transactionId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GetTransactionById));
        }
    }

    /// <summary>
    /// Registers a new transaction from the "Criar nova transação" screen.
    /// </summary>
    /// <remarks>
    /// Calculates <c>estimatedPoints</c> from total value × points-per-real,
    /// validates that the loyalty program is active, and persists any attached media.
    /// </remarks>
    /// <response code="201">Transaction created successfully. Returns the transaction detail.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Referenced loyalty program not found or inactive.</response>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TransactionDetailResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPost("transactions")]
    public async Task<ActionResult<TransactionDetailResponseDto>> CreateTransaction(
        [FromRoute] Guid userId,
        [FromBody] CreateTransactionRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("CreateTransaction called for user {UserId}", userId);

        try
        {
            ApiResult<TransactionDetailResponseDto> apiResult = await _transactionService.CreateAsync(userId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(CreateTransaction));
        }
    }

    /// <summary>
    /// Applies partial edits to a pending transaction.
    /// </summary>
    /// <remarks>
    /// Only transactions in <c>Pending</c> status may be edited.
    /// Recalculates estimated points when value or points-per-real changes.
    /// </remarks>
    /// <response code="200">Transaction updated successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    /// <response code="409">Transaction is not in Pending status and cannot be edited.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionDetailResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPut("transactions/{transactionId:guid}")]
    public async Task<ActionResult<TransactionDetailResponseDto>> UpdateTransaction(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId,
        [FromBody] UpdateTransactionRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("UpdateTransaction called for transaction {TransactionId} by user {UserId}", transactionId, userId);

        try
        {
            ApiResult<TransactionDetailResponseDto> apiResult = await _transactionService.UpdateAsync(userId, transactionId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UpdateTransaction));
        }
    }

    /// <summary>
    /// Transitions a transaction's status via the "Atualizar status" action sheet.
    /// </summary>
    /// <remarks>
    /// Accepted transitions: Pending → Received, Pending → Disputed.
    /// Sets <c>actualReceivedPoints</c> when marking as Received.
    /// Dispatches a push notification to the user on success.
    /// </remarks>
    /// <response code="200">Status updated successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    /// <response code="409">The requested status transition is not allowed.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionDetailResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPatch("transactions/{transactionId:guid}/status")]
    public async Task<ActionResult<TransactionDetailResponseDto>> UpdateTransactionStatus(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId,
        [FromBody] UpdateTransactionStatusRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("UpdateTransactionStatus called for transaction {TransactionId} by user {UserId}", transactionId, userId);

        try
        {
            ApiResult<TransactionDetailResponseDto> apiResult = await _transactionService.UpdateStatusAsync(userId, transactionId, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UpdateTransactionStatus));
        }
    }

    /// <summary>
    /// Deletes a transaction. Only the owning user may delete their own transactions.
    /// </summary>
    /// <response code="200">Transaction deleted successfully. Returns the deleted transaction detail.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionDetailResponseDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpDelete("transactions/{transactionId:guid}")]
    public async Task<ActionResult<TransactionDetailResponseDto>> DeleteTransaction(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("DeleteTransaction called for transaction {TransactionId} by user {UserId}", transactionId, userId);

        try
        {
            ApiResult<TransactionDetailResponseDto> apiResult = await _transactionService.DeleteAsync(userId, transactionId, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(DeleteTransaction));
        }
    }

    // ── Media ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Uploads a file to storage and adds a new media entry to an existing transaction.
    /// </summary>
    /// <remarks>
    /// The controller uploads the physical file to the storage bucket (MinIO/S3) first,
    /// then passes the resulting URL to the transaction service for persistence.
    /// Ownership is validated before any write occurs.
    /// </remarks>
    /// <response code="201">Media added successfully. Returns the new media entry.</response>
    /// <response code="400">Bad arguments passed or no file provided.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(TransactionMediaDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPost("transactions/{transactionId:guid}/media")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<TransactionMediaDto>> AddMedia(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId,
        [FromForm] AddMediaRequestDto request)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("AddMedia called for transaction {TransactionId} by user {UserId}", transactionId, userId);

        try
        {
            if (request.File == null || request.File.Length == 0)
            {
                return BadRequest(new ErrorApiResult
                (
                    InternalResultCode.MISSING_INFORMATION,
                    (int)HttpStatusCode.BadRequest,
                    "Nenhum arquivo foi fornecido."
                ));
            }

            // 1. Upload the physical file to the storage bucket and obtain the public URL
            string fileUrl = await _storageService.UploadFileAsync(request.File, userId, transactionId);

            // 2. Build the DTO with the resulting URL and persist via the transaction service
            UpsertTransactionMediaRequestDto mediaDto = new UpsertTransactionMediaRequestDto
            {
                TransactionMediaFileUrl = fileUrl,
                TransactionMediaFileType = request.File.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? TransactionMediaFileType.Pdf : TransactionMediaFileType.Image
            };

            ApiResult<TransactionMediaDto> apiResult = await _transactionService.AddMediaAsync(userId, transactionId, mediaDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(AddMedia));
        }
    }

    /// <summary>
    /// Replaces all media entries for a transaction atomically.
    /// </summary>
    /// <remarks>
    /// Uploads all provided files to the storage bucket, then replaces the full media list
    /// in a single database operation — equivalent to tapping "Salvar" on the Mídias screen.
    /// </remarks>
    /// <response code="200">Media replaced successfully. Returns the new media list.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Transaction not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(IEnumerable<TransactionMediaDto>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPut("transactions/{transactionId:guid}/media")]
    [Consumes("multipart/form-data")]
    public async Task<ActionResult<IEnumerable<TransactionMediaDto>>> BulkReplaceMedia(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId,
        [FromForm] IFormFileCollection files)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("BulkReplaceMedia called for transaction {TransactionId} by user {UserId}", transactionId, userId);

        try
        {
            // Upload every file to the bucket and collect the resulting public URLs
            List<UpsertTransactionMediaRequestDto> mediaDtos = new List<UpsertTransactionMediaRequestDto>();
            foreach (IFormFile file in files)
            {
                string fileUrl = await _storageService.UploadFileAsync(file, userId, transactionId);
                mediaDtos.Add(new UpsertTransactionMediaRequestDto
                {
                    TransactionMediaFileUrl = fileUrl,
                    TransactionMediaFileType = file.FileName.EndsWith(".pdf", StringComparison.OrdinalIgnoreCase) ? TransactionMediaFileType.Pdf : TransactionMediaFileType.Image
                });
            }

            ApiResult<IEnumerable<TransactionMediaDto>> apiResult = await _transactionService.BulkReplaceMediaAsync(userId, transactionId, mediaDtos, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(BulkReplaceMedia));
        }
    }

    /// <summary>
    /// Removes a single media entry from a transaction and deletes the physical file from storage.
    /// </summary>
    /// <remarks>
    /// The controller orchestrates both operations: the database record is removed first,
    /// then the physical file is deleted from the storage bucket to avoid orphaned objects.
    /// </remarks>
    /// <response code="200">Media deleted successfully. Returns the deleted media entry.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="403">Transaction does not belong to the requesting user.</response>
    /// <response code="404">Media entry or transaction not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(TransactionMediaDto))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status403Forbidden, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpDelete("transactions/{transactionId:guid}/media/{mediaId:guid}")]
    public async Task<ActionResult<TransactionMediaDto>> DeleteMedia(
        [FromRoute] Guid userId,
        [FromRoute] Guid transactionId,
        [FromRoute] Guid mediaId)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        _logger.LogInformation("DeleteMedia called for media {MediaId} on transaction {TransactionId} by user {UserId}", mediaId, transactionId, userId);

        try
        {
            // 1. Remove the database record and retrieve the deleted entry (which holds the file URL)
            ApiResult<TransactionMediaDto> apiResult = await _transactionService.DeleteMediaAsync(userId, transactionId, mediaId, currentUserId.Value);

            if (apiResult.HttpCode == System.Net.HttpStatusCode.OK && apiResult.Data?.TransactionMediaFileUrl != null)
            {
                await _storageService.DeleteFileAsync(apiResult.Data.TransactionMediaFileUrl);
            }

            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(DeleteMedia));
        }
    }
}
