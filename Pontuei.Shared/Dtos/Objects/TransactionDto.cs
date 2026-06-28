using System.Text.Json.Serialization;
using Pontuei.Shared.Enums;

namespace Pontuei.Shared.Dtos.Objects;

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
    [JsonPropertyName("transactionId")]
    public Guid TransactionId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("loyaltyProgramId")]
    public int LoyaltyProgramId { get; set; }

    [JsonPropertyName("transactionDescription")]
    public required string TransactionDescription { get; set; }

    [JsonPropertyName("transactionStore")]
    public required string TransactionStore { get; set; }

    [JsonPropertyName("transactionTotalValue")]
    public decimal TransactionTotalValue { get; set; }

    [JsonPropertyName("transactionPointsPerReal")]
    public short TransactionPointsPerReal { get; set; }

    [JsonPropertyName("transactionEstimatedPoints")]
    public int TransactionEstimatedPoints { get; set; }

    [JsonPropertyName("transactionActualReceivedPoints")]
    public int TransactionActualReceivedPoints { get; set; }

    [JsonPropertyName("transactionPurchaseDate")]
    public DateOnly TransactionPurchaseDate { get; set; }

    [JsonPropertyName("transactionItemReceiptDate")]
    public DateOnly? TransactionItemReceiptDate { get; set; }

    [JsonPropertyName("transactionReceiptDeadlineDays")]
    public short TransactionReceiptDeadlineDays { get; set; }

    [JsonPropertyName("transactionStatus")]
    public TransactionStatus TransactionStatus { get; set; }

    [JsonPropertyName("transactionStatusUpdatedAt")]
    public DateTime? TransactionStatusUpdatedAt { get; set; }

    [JsonPropertyName("deadline")]
    public DateOnly Deadline { get; set; }

    [JsonPropertyName("isOverdue")]
    public bool IsOverdue { get; set; }
}