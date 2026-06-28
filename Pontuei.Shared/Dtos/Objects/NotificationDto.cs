using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Objects;

/// <summary>
/// Notification item as displayed in the "Notificações" screen.
/// Each row shows the message, the associated points amount, and the date.
/// Unread notifications are visually highlighted with a bold border.
/// </summary>
public class NotificationDto
{
    [JsonPropertyName("notificationId")]
    public Guid NotificationId { get; set; }

    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("transactionId")]
    public Guid? TransactionId { get; set; }

    [JsonPropertyName("loyaltyProgramId")]
    public int? LoyaltyProgramId { get; set; }

    [JsonPropertyName("notificationMessage")]
    public required string NotificationMessage { get; set; }

    [JsonPropertyName("notificationPointsAmount")]
    public int? NotificationPointsAmount { get; set; }

    [JsonPropertyName("notificationIsRead")]
    public bool NotificationIsRead { get; set; }

    [JsonPropertyName("notificationReadAt")]
    public DateTime? NotificationReadAt { get; set; }
}