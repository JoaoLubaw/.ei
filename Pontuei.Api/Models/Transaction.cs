using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

using Pontuei.Api.Enums;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents a transaction made by a user, including details about the purchase, associated loyalty program, 
/// and media files related to the transaction.
/// </summary>
[Table("transaction"), DataContract]
public class Transaction
{
    [Column("transaction_id"), DataMember]
    public Guid TransactionId { get; set; }

    /// <summary>
    /// Unique identifier for the user who made the transaction.
    /// </summary>
    [Column("user_id"), DataMember]
    public Guid UserId { get; set; }

    [ForeignKey("UserId"), IgnoreDataMember]
    public virtual User? User { get; set; }

    /// <summary>
    /// Unique identifier for the loyalty program associated with the transaction.
    /// This field establishes a relationship between the transaction and the loyalty program, allowing for tracking of user-specific interactions and rewards within the context of that program.
    /// </summary>
    [Column("loyalty_program_id"), DataMember]
    public int LoyaltyProgramId { get; set; }

    [ForeignKey("LoyaltyProgramId"), IgnoreDataMember]
    public virtual LoyaltyProgram? LoyaltyProgram { get; set; }

    /// <summary>
    /// The description of the transaction, which is a required field and provides details about the purchase, such as items bought or services rendered.
    /// </summary>
    [Column("transaction_description"), DataMember]
    public required string TransactionDescription { get; set; }

    /// <summary>
    /// The store where the transaction took place, which is a required field and indicates the location or vendor associated with the purchase.   
    /// </summary>
    [Column("transaction_store"), DataMember]
    public required string TransactionStore { get; set; }

    /// <summary>
    /// The total value of the transaction, which is a required field and represents the monetary amount spent during the purchase. 
    /// This value is used to calculate loyalty points and track user spending within the loyalty program.
    /// </summary>
    [Column("transaction_total_value"), DataMember]
    public decimal TransactionTotalValue { get; set; }

    /// <summary>
    /// The date of the transaction purchase, which is a required field and indicates when the purchase was made.
    /// </summary>
    [Column("transaction_purchase_date"), DataMember]
    public DateOnly TransactionPurchaseDate { get; set; }

    /// <summary>
    /// The date when the transaction item was received, which is an optional field and can be used to track the time taken for item processing or verification.
    /// </summary>
    [Column("transaction_item_receipt_date"), DataMember]
    public DateOnly? TransactionItemReceiptDate { get; set; }

    /// <summary>
    /// The number of days allowed for transaction receipt, which is a required field and determines the deadline for item verification.
    /// </summary>
    [Column("transaction_receipt_deadline_days"), DataMember]
    public short TransactionReceiptDeadlineDays { get; set; }

    /// <summary>
    /// The number of loyalty points earned per unit of currency spent in the transaction, which is a required field and is used to calculate the total points awarded to the user based on their spending.
    /// </summary>
    [Column("transaction_points_per_real"), DataMember]
    public short TransactionPointsPerReal { get; set; }

    /// <summary>
    /// The current status of the transaction, which is a required field and indicates whether the transaction is pending, approved, or rejected.
    /// </summary>
    [Column("transaction_status"), DataMember]
    public TransactionStatus TransactionStatus { get; set; }

    /// <summary>
    /// The date and time when the transaction status was last updated, which is an optional field and can be used to track changes in the transaction's approval or rejection process.
    /// </summary>
    [Column("transaction_status_updated_at"), DataMember]
    public DateTime? TransactionStatusUpdatedAt { get; set; }

    /// <summary>
    /// Gets the deadline for the transaction receipt based on the purchase date and the allowed number of days for receipt.
    /// This property is calculated by adding the TransactionReceiptDeadlineDays to the TransactionPurchaseDate, 
    /// and it is used to determine if the transaction is overdue for item verification.
    /// </summary>
    [IgnoreDataMember]
    public DateOnly Deadline => TransactionPurchaseDate.AddDays(TransactionReceiptDeadlineDays);

    /// <summary>
    /// Gets a value indicating whether the transaction is overdue based on its status and the calculated deadline.
    /// A transaction is considered overdue if its status is pending and the current date exceeds the calculated deadline for item receipt. 
    /// This property is used to identify transactions that require attention or follow-up for verification.
    /// </summary>
    [IgnoreDataMember]
    public bool IsOverdue => TransactionStatus == TransactionStatus.Pending && DateOnly.FromDateTime(DateTime.UtcNow) > Deadline;

    /// <summary>
    /// Gets the estimated loyalty points earned from the transaction based on the total value and the points per unit of currency spent.
    /// This property calculates the total points by multiplying the TransactionTotalValue by the TransactionPointsPerReal, 
    /// and it is used to provide users with an estimate of the rewards they will receive for their purchase.
    /// </summary>
    [IgnoreDataMember]
    public int EstimatedPoints => (int)(TransactionTotalValue * TransactionPointsPerReal);

    [IgnoreDataMember]
    public virtual ICollection<TransactionMedia> TransactionMedias { get; set; } = new List<TransactionMedia>();
}