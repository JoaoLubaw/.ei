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

