using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Full transaction detail returned by GET /transactions/{id} and used
/// on the "Detalhes de transação" screen.
/// </summary>
public class TransactionDetailResponseDto
{
    /// <summary>
    /// Transaction unique identifier.
    /// </summary>
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Description of the purchased item.
    /// </summary>
    [JsonPropertyName("transactionDescription")]
    public string TransactionDescription { get; set; } = null!;

    /// <summary>
    /// Store where the purchase was made.
    /// </summary>
    [JsonPropertyName("transactionStore")]
    public string TransactionStore { get; set; } = null!;

    /// <summary>
    /// Total monetary value in BRL.
    /// </summary>
    [JsonPropertyName("transactionTotalValue")]
    public decimal TransactionTotalValue { get; set; }

    /// <summary>
    /// Date of purchase.
    /// </summary>
    [JsonPropertyName("transactionPurchaseDate")]
    public DateOnly TransactionPurchaseDate { get; set; }

    /// <summary>
    /// Date the item was physically received, if already set.
    /// </summary>
    [JsonPropertyName("transactionItemReceiptDate")]
    public DateOnly? TransactionItemReceiptDate { get; set; }

    /// <summary>
    /// Maximum days allowed for points posting.
    /// </summary>
    [JsonPropertyName("transactionReceiptDeadlineDays")]
    public short TransactionReceiptDeadlineDays { get; set; }

    /// <summary>
    /// Calculated deadline date (purchase date + deadline days).
    /// </summary>
    [JsonPropertyName("deadline")]
    public DateOnly Deadline { get; set; }

    /// <summary>
    /// Whether the transaction is past its deadline and still pending.
    /// </summary>
    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }

    /// <summary>
    /// Points awarded per BRL 1.00 on the day of purchase.
    /// </summary>
    [JsonPropertyName("transactionPointsPerReal")]
    public short TransactionPointsPerReal { get; set; }

    /// <summary>
    /// Estimated total points based on value × points-per-real.
    /// </summary>
    [JsonPropertyName("transactionEstimatedPoints")]
    public int TransactionEstimatedPoints { get; set; }

    /// <summary>
    /// Actual points credited by the program, once confirmed.
    /// </summary>
    [JsonPropertyName("transactionActualReceivedPoints")]
    public int TransactionActualReceivedPoints { get; set; }

    /// <summary>
    /// Current status of the transaction.
    /// </summary>
    [JsonPropertyName("transactionStatus")]
    public TransactionStatus TransactionStatus { get; set; }

    /// <summary>
    /// Date-time of the last status change.
    /// </summary>
    [JsonPropertyName("transactionStatusUpdatedAt")]
    public DateTime? TransactionStatusUpdatedAt { get; set; }

    /// <summary>
    /// Associated loyalty program summary for card rendering.
    /// </summary>
    [JsonPropertyName("loyaltyProgram")]
    public LoyaltyProgramDto LoyaltyProgram { get; set; } = null!;

    /// <summary>
    /// Attached media files (receipts, screenshots) ordered by display order.
    /// Shown in the "Mídias" section of the detail screen.
    /// </summary>
    [JsonPropertyName("medias")]
    public List<TransactionMediaDto> Medias { get; set; } = [];
}

/// <summary>
/// Compact transaction summary used in list views:
/// the home-screen "Transações esperadas" list and the history screen.
/// </summary>
public class TransactionDto
{
    /// <summary>Transaction unique identifier.</summary>
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    /// <summary>Description of the purchased item (e.g., "TV").</summary>
    [JsonPropertyName("transactionDescription")]
    public string TransactionDescription { get; set; } = null!;

    /// <summary>Store name shown as the subtitle in the list row.</summary>
    [JsonPropertyName("transactionStore")]
    public string TransactionStore { get; set; } = null!;

    /// <summary>Estimated points shown on the right side of each list row.</summary>
    [JsonPropertyName("transactionEstimatedPoints")]
    public int TransactionEstimatedPoints { get; set; }

    /// <summary>Deadline date shown below the points on each list row.</summary>
    [JsonPropertyName("deadline")]
    public DateOnly Deadline { get; set; }

    /// <summary>Whether the transaction is past its deadline and still pending.</summary>
    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }

    /// <summary>Current status, used to apply the red overdue border in the UI.</summary>
    [JsonPropertyName("transactionStatus")]
    public TransactionStatus TransactionStatus { get; set; }

    /// <summary>
    /// Program summary used to render the program logo/chip on the history screen.
    /// </summary>
    [JsonPropertyName("loyaltyProgram")]
    public LoyaltyProgramDto LoyaltyProgram { get; set; } = null!;
}

