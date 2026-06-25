using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for marking a single notification as read.
/// Sent when the user taps a notification row.
/// </summary>
public class MarkNotificationReadRequestDto
{
    /// <summary>
    /// UTC date-time when the user opened the notification.
    /// Stored in <c>notification_read_at</c>.
    /// Defaults to now when omitted, but the client can supply the exact tap timestamp.
    /// </summary>
    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }
}

// ─────────────────────────────────────────────
//  Mark all as read
//  (PATCH /notifications/read-all)
// ─────────────────────────────────────────────

/// <summary>
/// Payload for bulk-marking all unread notifications as read.
/// Triggered by the "Marcar todas as notificações como lidas" link at the bottom
/// of the Notificações screen.
/// </summary>
public class MarkAllNotificationsReadRequestDto
{
    /// <summary>
    /// UTC date-time used as <c>notification_read_at</c> for all affected rows.
    /// Defaults to server time when omitted.
    /// </summary>
    [JsonPropertyName("readAt")]
    public DateTime? ReadAt { get; set; }
}

/// <summary>
/// Result returned after a bulk mark-as-read operation.
/// </summary>
public class MarkAllNotificationsReadResponseDto
{
    /// <summary>Number of notification rows that were updated.</summary>
    [JsonPropertyName("updatedCount")]
    public int UpdatedCount { get; set; }
}
