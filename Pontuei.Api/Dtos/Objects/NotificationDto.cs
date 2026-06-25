using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Notification item as displayed in the "Notificações" screen.
/// Each row shows the message, the associated points amount, and the date.
/// Unread notifications are visually highlighted with a bold border.
/// </summary>
public class NotificationDto
{
    /// <summary>
    /// Notification unique identifier.
    /// </summary>
    [JsonPropertyName("notificationId")]
    public Guid NotificationId { get; set; }

    /// <summary>
    /// Full notification message text.
    /// Example: "O prazo de receber seus pontos {Livelo} 10.000pts expiraram, que tal atualizar?"
    /// </summary>
    [JsonPropertyName("notificationMessage")]
    public string NotificationMessage { get; set; } = null!;

    /// <summary>
    /// Points amount highlighted on the right side of the notification row (e.g., 10.000 pts).
    /// Null when the notification is not directly related to a point balance change.
    /// </summary>
    [JsonPropertyName("notificationPointsAmount")]
    public int? NotificationPointsAmount { get; set; }

    /// <summary>
    /// Whether the user has already read this notification.
    /// </summary>
    [JsonPropertyName("notificationIsRead")]
    public bool NotificationIsRead { get; set; }

    /// <summary>
    /// UTC date-time when the notification was read, or null if still unread.
    /// </summary>
    [JsonPropertyName("notificationReadAt")]
    public DateTime? NotificationReadAt { get; set; }

    /// <summary>
    /// Compact transaction reference, when the notification is linked to a specific transaction.
    /// Allows the client to navigate directly to the transaction detail screen on tap.
    /// </summary>
    [JsonPropertyName("transaction")]
    public TransactionDto? Transaction { get; set; }

    /// <summary>
    /// Compact loyalty program reference, when the notification is linked to a program.
    /// Used to render the program logo chip beside the notification message.
    /// </summary>
    [JsonPropertyName("loyaltyProgram")]
    public LoyaltyProgramDto? LoyaltyProgram { get; set; }
}
