using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Requests;

/// <summary>
/// Payload for exchanging a valid refresh token for a new access/refresh token pair.
/// The old session row is invalidated and a new <c>UserSession</c> is created.
/// </summary>
public class RefreshTokenRequestDto
{
    /// <summary>
    /// The refresh token previously issued at login.
    /// Maps to <c>user_session_refresh_token</c>.
    /// </summary>
    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// Optional updated device info to keep the session row current.
    /// </summary>
    [JsonPropertyName("deviceInfo")]
    public string? DeviceInfo { get; set; }
}

/// <summary>
/// Payload for registering or updating the device push-notification token
/// associated with the current session.
/// Maps to <c>user_session_push_notification_token</c>.
/// </summary>
public class UpdatePushNotificationTokenRequestDto
{
    /// <summary>
    /// FCM / APNs token obtained by the mobile client.
    /// Pass <c>null</c> to clear the token (e.g., on logout).
    /// </summary>
    [JsonPropertyName("pushNotificationToken")]
    public string? PushNotificationToken { get; set; }
}
