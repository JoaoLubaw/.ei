using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for confirming the user's e-mail address with the 6-digit code
/// sent after registration. Matches the "Confirme seu email" screen in the prototype.
/// </summary>
public class VerifyEmailRequestDto
{
    /// <summary>
    /// The 6-digit verification code entered by the user.
    /// Compared against the hashed value in <c>verification_code_hash</c>
    /// where <c>verification_code_type = EmailConfirmation</c>.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}

/// <summary>
/// Payload for re-sending the e-mail verification code.
/// Triggered by "Por aqui não recebi nada... (reenviar)" after the 60-second cooldown.
/// </summary>
public class ResendVerificationEmailRequestDto
{
    /// <summary>
    /// E-mail address to which the new code should be sent.
    /// Must match the address on the user's account.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }
}
