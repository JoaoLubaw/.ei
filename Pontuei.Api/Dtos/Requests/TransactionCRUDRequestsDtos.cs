using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for registering a new loyalty-points transaction.
/// Matches the "Criar nova transação" screen fields:
/// description, store, total value, purchase date, item receipt date,
/// receipt deadline (days), points per real, and loyalty program.
/// </summary>
public class CreateTransactionRequestDto
{
    /// <summary>
    /// Short description of the purchased item (e.g., "TV").
    /// Displayed as the transaction title in lists and detail views.
    /// Maps to <c>transaction_description</c>.
    /// </summary>
    [JsonPropertyName("transactionDescription")]
    public required string TransactionDescription { get; set; }

    /// <summary>
    /// Name of the store or vendor where the purchase was made (e.g., "Casas Bahia").
    /// Maps to <c>transaction_store</c>.
    /// </summary>
    [JsonPropertyName("transactionStore")]
    public required string TransactionStore { get; set; }

    /// <summary>
    /// Total monetary value of the purchase in BRL.
    /// Used together with <see cref="TransactionPointsPerReal"/> to calculate
    /// <c>transaction_estimated_points</c>.
    /// Maps to <c>transaction_total_value</c>.
    /// </summary>
    [JsonPropertyName("transactionTotalValue")]
    public decimal TransactionTotalValue { get; set; }

    /// <summary>
    /// The date the purchase was made.
    /// Maps to <c>transaction_purchase_date</c>.
    /// </summary>
    [JsonPropertyName("transactionPurchaseDate")]
    public DateOnly TransactionPurchaseDate { get; set; }

    /// <summary>
    /// The date the purchased item was physically received, when already known at creation time.
    /// Optional. Maps to <c>transaction_item_receipt_date</c>.
    /// </summary>
    [JsonPropertyName("transactionItemReceiptDate")]
    public DateOnly? TransactionItemReceiptDate { get; set; }

    /// <summary>
    /// Maximum number of days the loyalty program allows for points to be posted
    /// after purchase. Determines the <c>Deadline</c> computed property.
    /// Shown as "Prazo de recebimento" with +/- stepper in the prototype.
    /// Maps to <c>transaction_receipt_deadline_days</c>.
    /// </summary>
    [JsonPropertyName("transactionReceiptDeadlineDays")]
    public short TransactionReceiptDeadlineDays { get; set; } = 30;

    /// <summary>
    /// Points awarded per BRL 1.00 spent, as advertised by the loyalty program
    /// on the day of purchase. Shown as "Pontuação do dia (por real)" in the prototype.
    /// Maps to <c>transaction_points_per_real</c>.
    /// </summary>
    [JsonPropertyName("transactionPointsPerReal")]
    public decimal TransactionPointsPerReal { get; set; }

    /// <summary>
    /// Identifier of the loyalty program that will credit the points.
    /// Selected via the "Programa de pontuação" program picker screen.
    /// Maps to <c>loyalty_program_id</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramId")]
    public int LoyaltyProgramId { get; set; }

    /// <summary>
    /// Optional media files (receipt images, screenshots) attached at creation time.
    /// Additional files can be added later via the media endpoint.
    /// </summary>
    [JsonPropertyName("medias")]
    public List<UpsertTransactionMediaRequestDto>? Medias { get; set; }

    /// <summary>
    /// Returns <c>true</c> when all required fields are present and value is positive.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(TransactionDescription) &&
        !string.IsNullOrWhiteSpace(TransactionStore) &&
        TransactionTotalValue > 0 &&
        TransactionPointsPerReal > 0 &&
        TransactionReceiptDeadlineDays > 0 &&
        LoyaltyProgramId > 0;
}

/// <summary>
/// Payload for editing a pending transaction's mutable fields.
/// Accessed via the pencil icon on the "Detalhes de transação" screen.
/// All fields are optional — only non-null values are applied.
/// Status changes use a dedicated endpoint (<see cref="UpdateTransactionStatusRequestDto"/>).
/// </summary>
public class UpdateTransactionRequestDto
{
    /// <summary>
    /// Updated purchase description. Optional.
    /// </summary>
    [JsonPropertyName("transactionDescription")]
    public string? TransactionDescription { get; set; }

    /// <summary>
    /// Updated store name. Optional.
    /// </summary>
    [JsonPropertyName("transactionStore")]
    public string? TransactionStore { get; set; }

    /// <summary>
    /// Updated total value in BRL. Optional.
    /// </summary>
    [JsonPropertyName("transactionTotalValue")]
    public decimal? TransactionTotalValue { get; set; }

    /// <summary>
    /// Updated purchase date. Optional.
    /// </summary>
    [JsonPropertyName("transactionPurchaseDate")]
    public DateOnly? TransactionPurchaseDate { get; set; }

    /// <summary>
    /// Updated item receipt date. Optional.
    /// </summary>
    [JsonPropertyName("transactionItemReceiptDate")]
    public DateOnly? TransactionItemReceiptDate { get; set; }

    /// <summary>
    /// Updated receipt deadline in days..
    /// </summary>
    [JsonPropertyName("transactionReceiptDeadlineDays")]
    public short? TransactionReceiptDeadlineDays { get; set; }

    /// <summary>
    /// Updated points-per-real rate. Optional.
    /// </summary>
    [JsonPropertyName("transactionPointsPerReal")]
    public short? TransactionPointsPerReal { get; set; }

    /// <summary>
    /// Updated loyalty program selection.
    /// Optional — allows correcting a mis-assigned program.
    /// </summary>
    [JsonPropertyName("loyaltyProgramId")]
    public int? LoyaltyProgramId { get; set; }
}
/// <summary>
/// Payload for updating a transaction's status.
/// Matches the "Atualizar status" action sheet on the detail screen, which presents
/// "Recebido" (points arrived) and "Contestado" (points delayed/missing) options
/// along with an effective date picker.
/// </summary>
public class UpdateTransactionStatusRequestDto
{
    /// <summary>
    /// The new status to apply to the transaction.
    /// Accepted values from the prototype: <c>Received</c> ("Recebido") or
    /// <c>Disputed</c> ("Contestado").
    /// Maps to <c>transaction_status</c>.
    /// </summary>
    [JsonPropertyName("transactionStatus")]
    public required TransactionStatus TransactionStatus { get; set; }

    /// <summary>
    /// Actual number of points received, required when status is <c>Received</c>.
    /// May differ from <c>transaction_estimated_points</c> due to program adjustments.
    /// Maps to <c>transaction_actual_received_points</c>.
    /// </summary>
    [JsonPropertyName("transactionActualReceivedPoints")]
    public decimal? TransactionActualReceivedPoints { get; set; }
}

public class GetTransactionsRequestDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public TransactionFiltersDto? Filters { get; set; }
}

public class TransactionFiltersDto
{
    public Guid? TransactionId { get; set; }
    public Guid? UserId { get; set; }
    public int? LoyaltyProgramId { get; set; }
    public string? TransactionStore { get; set; }
    public TransactionStatus? TransactionStatus { get; set; }
    public DateOnly? TransactionPurchaseDate { get; set; }
    public bool? IsOverdue { get; set; }
}