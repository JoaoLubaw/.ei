using System.Text.Json.Serialization;
using Pontuei.Shared.Dtos.Objects;

namespace Pontuei.Shared.Dtos.Responses;

public class LoginResponseDto
{
    /// <summary>
    /// Short-lived JWT access token sent in the Authorization header.
    /// </summary>
    [JsonPropertyName("accessToken")]
    public required string AccessToken { get; set; }

    /// <summary>
    /// Long-lived refresh token stored in <c>user_session_refresh_token</c>.
    /// Used to obtain a new access token when the current one expires.
    /// </summary>
    [JsonPropertyName("refreshToken")]
    public required string RefreshToken { get; set; }

    /// <summary>
    /// UTC date-time when the refresh token expires.
    /// </summary>
    [JsonPropertyName("refreshTokenExpiresAt")]
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Basic user information so the client can render the UI without an extra request.
    /// </summary>
    [JsonPropertyName("user")]
    public required UserDto User { get; set; }

    /// <summary>
    /// Unique identifier of the user session, used to identify the session in subsequent requests.
    /// </summary>
    [JsonPropertyName("sessionId")]
    public required Guid SessionId { get; set; }
}