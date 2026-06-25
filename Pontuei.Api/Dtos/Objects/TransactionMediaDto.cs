using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Represents a single media attachment within a transaction response.
/// </summary>
public class TransactionMediaDto
{
    /// <summary>
    /// Media entry identifier.
    /// </summary>
    [JsonPropertyName("transactionMediaId")]
    public Guid TransactionMediaId { get; set; }

    /// <summary>
    /// Public URL of the stored file (image, PDF receipt, etc.).
    /// Shown in the "Mídias" grid on the transaction detail screen.
    /// </summary>
    [JsonPropertyName("transactionMediaFileUrl")]
    public string? TransactionMediaFileUrl { get; set; }

    /// <summary>
    /// File type enum value (e.g., Image, Pdf).
    /// </summary>
    [JsonPropertyName("transactionMediaFileType")]
    public TransactionMediaFileType TransactionMediaFileType { get; set; }

    /// <summary>
    /// Position in the media grid (ascending order = left-to-right, top-to-bottom).
    /// </summary>
    [JsonPropertyName("transactionMediaDisplayOrder")]
    public short TransactionMediaDisplayOrder { get; set; }
}