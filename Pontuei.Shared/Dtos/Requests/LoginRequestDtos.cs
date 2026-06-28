using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Requests;

/// <summary>
/// Payload for authenticating with e-mail and password.
/// </summary>
public class LoginRequestDto
{
    /// <summary>
    /// E-mail address registered to the account.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }

    /// <summary>
    /// Plain-text password to be verified against the stored hash.
    /// </summary>
    [JsonPropertyName("password")]
    public required string Password { get; set; }

    /// <summary>
    /// When <c>true</c>, the issued refresh token receives an extended expiration
    /// (long-lived session). Corresponds to the "Se lembre de mim" toggle.
    /// </summary>
    [JsonPropertyName("rememberMe")]
    public bool RememberMe { get; set; } = false;

    /// <summary>
    /// Optional device description (model, OS) stored in <c>user_session_device_info</c>
    /// for security-audit purposes.
    /// </summary>
    [JsonPropertyName("deviceInfo")]
    public string? DeviceInfo { get; set; }

    /// <summary>
    /// Returns <c>true</c> when both required fields are non-empty.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(UserEmail) &&
        !string.IsNullOrWhiteSpace(Password);
}

/// <summary>
/// Payload for authenticating via Google OAuth.
/// The backend exchanges the ID token for a user account (create or fetch)
/// and returns the same token pair as a regular login.
/// </summary>
public class GoogleLoginRequestDto
{
    /// <summary>
    /// Google ID token obtained from the client-side Google Sign-In SDK.
    /// The backend validates this token against Google's public keys and extracts
    /// the <c>sub</c> claim to populate <c>user_google_id</c>.
    /// </summary>
    [JsonPropertyName("idToken")]
    public required string IdToken { get; set; }

    /// <summary>
    /// Optional device description forwarded to <c>user_session_device_info</c>.
    /// </summary>
    [JsonPropertyName("deviceInfo")]
    public string? DeviceInfo { get; set; }
}