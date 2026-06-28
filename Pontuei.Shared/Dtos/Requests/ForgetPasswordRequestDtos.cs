using System.Text.Json.Serialization;
using Pontuei.Shared.Common;

namespace Pontuei.Shared.Dtos.Requests;

/// <summary>
/// Payload for initiating the password-reset flow.
/// The user enters their registered
/// e-mail and the backend sends a 6-digit reset code.
/// </summary>
public class ForgotPasswordRequestDto
{
    /// <summary>
    /// Registered e-mail address. If an account is found, a reset code is sent.
    /// The response is always generic (no confirmation whether the account exists)
    /// to prevent user enumeration.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }

    /// <summary>
    /// Validates e-mail format before dispatching the request.
    /// </summary>
    public bool IsValidEmail() =>
        ValidationUtils.IsValidEmail(UserEmail);
}

/// <summary>
/// Payload for validating the 6-digit reset code before allowing the user to
/// set a new password. Matches the second "Esqueci minha senha" screen.
/// </summary>
public class VerifyResetCodeRequestDto
{
    /// <summary>
    /// The e-mail address that initiated the reset flow.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }

    /// <summary>
    /// The 6-digit code entered by the user.
    /// Validated against <c>verification_code_hash</c>
    /// where <c>verification_code_type = PasswordReset</c>.
    /// </summary>
    [JsonPropertyName("code")]
    public required string Code { get; set; }
}

/// <summary>
/// Payload for setting a new password after the reset code has been verified.
/// Matches the third "Esqueci minha senha" screen ("Prontinho, agora é só escolher a senha nova!").
/// </summary>
public class ResetPasswordRequestDto
{
    /// <summary>
    /// The single-use token obtained from .
    /// Ensures the request is coming from someone who completed the code-verification step.
    /// </summary>
    [JsonPropertyName("resetToken")]
    public required string ResetToken { get; set; }

    /// <summary>
    /// New plain-text password to be hashed and stored.
    /// </summary>
    [JsonPropertyName("newPassword")]
    public required string NewPassword { get; set; }

    /// <summary>
    /// Must match <see cref="NewPassword"/> exactly.
    /// </summary>
    [JsonPropertyName("confirmNewPassword")]
    public required string ConfirmNewPassword { get; set; }

    /// <summary>
    /// Returns <c>true</c> when the two password fields match.
    /// </summary>
    public bool PasswordsMatch() => NewPassword == ConfirmNewPassword;

    /// <summary>
    /// Validates the new password against the complexity rules shown in the prototype.
    /// </summary>
    public bool IsValidPassword() =>
        ValidationUtils.IsValidPassword(NewPassword);
}

/// <summary>
/// Payload for changing the authenticated user's password.
/// Shown via the "Alterar senha" button on the settings screen.
/// </summary>
public class ChangePasswordRequestDto
{
    /// <summary>
    /// The user's current password for verification before change.
    /// </summary>
    [JsonPropertyName("currentPassword")]
    public required string CurrentPassword { get; set; }

    /// <summary>
    /// The new password to be stored (will be hashed).
    /// </summary>
    [JsonPropertyName("newPassword")]
    public required string NewPassword { get; set; }

    /// <summary>
    /// Must match <see cref="NewPassword"/> exactly.
    /// </summary>
    [JsonPropertyName("confirmNewPassword")]
    public required string ConfirmNewPassword { get; set; }

    /// <summary>
    /// Returns <c>true</c> when new password and confirmation match.
    /// </summary>
    public bool PasswordsMatch() => NewPassword == ConfirmNewPassword;

    /// <summary>
    /// Validates the new password against the complexity rules shown in the prototype.
    /// </summary>
    public bool IsValidPassword() =>
        ValidationUtils.IsValidPassword(NewPassword);
}

