using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// Covers all transaction-related API endpoints:
///
/// Dashboard:
///   GET    /users/{userId}/dashboard
///
/// Transactions:
///   GET    /users/{userId}/transactions
///   GET    /users/{userId}/transactions/{transactionId}
///   POST   /users/{userId}/transactions
///   PATCH  /users/{userId}/transactions/{transactionId}
///   DELETE /users/{userId}/transactions/{transactionId}
///
/// Media:
///   POST   /users/{userId}/transactions/{transactionId}/media          (multipart)
///   PUT    /users/{userId}/transactions/{transactionId}/media          (multipart bulk)
///   DELETE /users/{userId}/transactions/{transactionId}/media/{mediaId}
/// </summary>
public class TransactionApiService
{
    private readonly ApiClient _api;

    public TransactionApiService(ApiClient api)
    {
        _api = api;
    }

    // ── Dashboard ─────────────────────────────────────────────────────────

    /// <summary>
    /// Home-page summary of the user's transactions and points, including total points, pending points, and recent transactions.
    /// </summary>
    public Task<ApiResponse<GetDashboardSummaryResponseDto>> GetDashboardSummaryAsync(Guid userId)
        => _api.GetAsync<GetDashboardSummaryResponseDto>($"users/{userId}/dashboard");

    // ── Transactions ────────────────────────────────────────────────────────

    /// <summary>
    /// Lists all transactions of a user, with optional filtering and pagination.
    /// </summary>
    public Task<ApiResponse<GetTransactionsResponseDto>> GetTransactionsAsync(
        Guid userId,
        GetTransactionsRequestDto? request = null)
    {
        string url = ApiClient.BuildQueryString($"users/{userId}/transactions", request);
        return _api.GetAsync<GetTransactionsResponseDto>(url);
    }

    /// <summary>
    /// Gets the details of a specific transaction, including attached media.
    /// </summary>
    public Task<ApiResponse<TransactionDetailResponseDto>> GetTransactionByIdAsync(
        Guid userId,
        Guid transactionId)
        => _api.GetAsync<TransactionDetailResponseDto>($"users/{userId}/transactions/{transactionId}");

    /// <summary>
    /// Registers a new transaction for the user. Returns the created transaction with its ID and status.
    /// </summary>
    public Task<ApiResponse<TransactionDetailResponseDto>> CreateTransactionAsync(
        Guid userId,
        CreateTransactionRequestDto request)
        => _api.PostAsync<TransactionDetailResponseDto>($"users/{userId}/transactions", request);

    /// <summary>
    /// Edits an existing transaction. Returns the updated transaction details. 
    /// </summary>
    public Task<ApiResponse<TransactionDetailResponseDto>> UpdateTransactionAsync(
        Guid userId,
        Guid transactionId,
        UpdateTransactionRequestDto request)
        => _api.PatchAsync<TransactionDetailResponseDto>($"users/{userId}/transactions/{transactionId}", request);

    /// <summary>
    /// Deletes a transaction and its associated media.
    /// </summary>
    public Task<ApiResponse<TransactionDetailResponseDto>> DeleteTransactionAsync(
        Guid userId,
        Guid transactionId)
        => _api.DeleteAsync<TransactionDetailResponseDto>($"users/{userId}/transactions/{transactionId}");

    // ── Media ─────────────────────────────────────────────────────────────

    /// <summary>
    /// Uploads a media file (image, PDF, etc.) to a specific transaction. Returns the created media details.
    /// </summary>
    public async Task<ApiResponse<TransactionMediaDto>> AddMediaAsync(
        Guid userId,
        Guid transactionId,
        Stream fileStream,
        string fileName,
        string contentType)
    {
        using MultipartFormDataContent content = new();
        StreamContent fileContent = new(fileStream);
        fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
        content.Add(fileContent, "file", fileName);

        return await _api.PostMultipartAsync<TransactionMediaDto>(
            $"users/{userId}/transactions/{transactionId}/media",
            content);
    }

    /// <summary>
    /// Replaces all media files of a transaction with a new set of files. Returns the updated list of media.
    /// This is equivalent to the "Save" action in the Media screen.
    /// </summary>
    public async Task<ApiResponse<IEnumerable<TransactionMediaDto>>> BulkReplaceMediaAsync(
        Guid userId,
        Guid transactionId,
        IEnumerable<(Stream stream, string fileName, string contentType)> files)
    {
        using MultipartFormDataContent content = new();

        foreach ((Stream stream, string fileName, string contentType) in files)
        {
            StreamContent fileContent = new(stream);
            fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            content.Add(fileContent, "files", fileName);
        }

        return await _api.PutMultipartAsync<IEnumerable<TransactionMediaDto>>(
            $"users/{userId}/transactions/{transactionId}/media",
            content);
    }

    /// <summary>
    /// Removes a specific media file from the transaction (and the physical file in storage).
    /// </summary>
    public Task<ApiResponse<TransactionMediaDto>> DeleteMediaAsync(
        Guid userId,
        Guid transactionId,
        Guid mediaId)
        => _api.DeleteAsync<TransactionMediaDto>(
            $"users/{userId}/transactions/{transactionId}/media/{mediaId}");

}