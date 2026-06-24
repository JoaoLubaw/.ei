using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;

namespace Pontuei.Api.Models;

/// <summary>
/// Represents a notification sent to a user, including details about the associated transaction, loyalty program, and read status.
/// </summary>
[Table("notification"), DataContract]
public class Notification
{
    [Column("notification_id"), DataMember]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Unique identifier for the user associated with this notification.
    /// </summary>
    [Column("user_id"), DataMember]
    public Guid UserId { get; set; }

    [ForeignKey("UserId"), IgnoreDataMember]
    public virtual User? User { get; set; }

    /// <summary>
    /// Unique identifier for the transaction associated with this notification, if applicable. 
    /// This field establishes a relationship between the notification and the transaction, allowing for tracking of user-specific 
    /// interactions and updates related to that transaction.
    /// </summary>
    [Column("transaction_id"), DataMember]
    public Guid? TransactionId { get; set; }

    [ForeignKey("TransactionId"), IgnoreDataMember]
    public virtual Transaction? Transaction { get; set; }

    /// <summary>
    /// Unique identifier for the loyalty program associated with this notification, if applicable.
    /// This field establishes a relationship between the notification and the loyalty program, enabling the system to track which loyalty programs 
    /// a user is enrolled in and manage their participation, preferences, and rewards within the context of that specific program.
    /// </summary>
    [Column("loyalty_program_id"), DataMember]
    public int? LoyaltyProgramId { get; set; }

    [ForeignKey("LoyaltyProgramId"), IgnoreDataMember]
    public virtual LoyaltyProgram? LoyaltyProgram { get; set; }

    /// <summary>
    /// Gets or sets the message content of the notification, which is a required field and provides information to the user about the associated transaction, 
    /// loyalty program, or other relevant updates.
    /// </summary>
    [Column("notification_message"), DataMember]
    public string NotificationMessage { get; set; } = null!;

    /// <summary>
    /// Gets or sets the amount of points associated with the notification, if applicable.
    /// This field can be used to inform the user about points earned, redeemed, or related to a specific transaction or loyalty program activity. 
    /// It is an optional field and may be null if no points are involved in the notification.
    /// </summary>
    [Column("notification_points_amount"), DataMember]
    public int? NotificationPointsAmount { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the notification has been read by the user.
    /// This field allows the system to track the read status of notifications, enabling users to manage their notifications and ensuring that important updates are acknowledged.
    /// </summary>
    [Column("notification_is_read"), DataMember]
    public bool NotificationIsRead { get; set; }

    /// <summary>
    /// Gets or sets the date and time when the notification was read by the user, which is an optional field and is null if the notification has not been read yet.
    /// This field can be used to track user engagement with notifications and provide insights into user behavior and
    /// </summary>
    [Column("notification_read_at"), DataMember]
    public DateTime? NotificationReadAt { get; set; }

}