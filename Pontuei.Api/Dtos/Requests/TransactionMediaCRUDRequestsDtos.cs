using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for adding or replacing a media file on an existing transaction.
/// Used when the user taps a "+" cell in the "Mídias" section.
/// File upload (multipart/form-data) is handled separately; this DTO receives
/// the resulting URL after the file has been stored by the upload service.
/// </summary>
public class UpsertTransactionMediaRequestDto
{
    /// <summary>
    /// URL of the already-uploaded file.
    /// Populated by the client after completing the upload step.
    /// </summary>
    [JsonPropertyName("transactionMediaFileUrl")]
    public required string TransactionMediaFileUrl { get; set; }

    /// <summary>
    /// Type of the media file.
    /// Maps to <c>transaction_media_file_type</c>.
    /// </summary>
    [JsonPropertyName("transactionMediaFileType")]
    public required TransactionMediaFileType TransactionMediaFileType { get; set; }

    /// <summary>
    /// Display order within the media grid.
    /// Maps to <c>transaction_media_display_order</c>.
    /// </summary>
    [JsonPropertyName("transactionMediaDisplayOrder")]
    public short TransactionMediaDisplayOrder { get; set; }
}

/// <summary>
/// Payload for retrieving a paginated list of media files associated with a transaction.
/// </summary>
public class GetTransactionMediasRequestDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public TransactionMediaFiltersDto? Filters { get; set; }
}

/// <summary>
/// Filters for retrieving a paginated list of media files associated with a transaction.
/// </summary>
public class TransactionMediaFiltersDto
{
    public Guid? TransactionMediaId { get; set; }
    public Guid? TransactionId { get; set; }
    public TransactionMediaFileType? TransactionMediaFileType { get; set; }
}

/// <summary>
/// Payload for adding a media file to an existing transaction.
/// </summary>
public class AddMediaRequestDto
{
    public IFormFile? File { get; set; }
}