using System.Text.Json.Serialization;
using Pontuei.Api.Dtos.Objects;

namespace Pontuei.Api.Dtos.Responses;

public class LoginResponseDto
{
    /// <summary>
    /// Short-lived JWT access token sent in the Authorization header.
    /// </summary>
    [JsonPropertyName("accessToken")]
    public string AccessToken { get; set; } = null!;

    /// <summary>
    /// Long-lived refresh token stored in <c>user_session_refresh_token</c>.
    /// Used to obtain a new access token when the current one expires.
    /// </summary>
    [JsonPropertyName("refreshToken")]
    public string RefreshToken { get; set; } = null!;

    /// <summary>
    /// UTC date-time when the refresh token expires.
    /// </summary>
    [JsonPropertyName("refreshTokenExpiresAt")]
    public DateTime RefreshTokenExpiresAt { get; set; }

    /// <summary>
    /// Basic user information so the client can render the UI without an extra request.
    /// </summary>
    [JsonPropertyName("user")]
    public UserBaseDto User { get; set; } = null!;
}
