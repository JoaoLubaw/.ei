using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Responses;

/// <summary>
/// Token returned after a valid reset code is confirmed.
/// Must be included in the subsequent call.
/// </summary>
public class VerifyResetCodeResponseDto
{
    /// <summary>
    /// Short-lived, single-use token that authorises the password-reset step.
    /// Prevents a user who only knows the e-mail from setting a new password
    /// without first verifying the code.
    /// </summary>
    [JsonPropertyName("resetToken")]
    public string ResetToken { get; set; } = null!;
}
