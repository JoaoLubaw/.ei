namespace Pontuei.Api.Enums;

/// <summary>
/// Represents the type of email verification code being sent to the user.
/// </summary>
public enum EmailVerificationCodeType
{
    /// <summary>
    /// Indicates that the email verification code is for confirming the user's email address.
    /// </summary>
    EmailConfirmation = 0,

    /// <summary>
    /// Indicates that the email verification code is for resetting the user's password.
    /// </summary>
    PasswordReset = 1
}