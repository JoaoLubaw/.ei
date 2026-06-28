namespace Pontuei.Shared.Enums;

/// <summary>
/// Represents the type of email verification code being sent to the user.
/// </summary>
public enum VerificationCodeType
{
    /// <summary>
    /// Indicates that the email verification code is for confirming the user's email address.
    /// </summary>
    EmailConfirmation = 0,

    /// <summary>
    /// Indicates that the email verification code is for resetting the user's password.
    /// </summary>
    PasswordReset = 1,

    /// <summary>
    /// Indicates that the email verification code is for confirming a change to the user's email address.
    /// </summary>
    EmailChangeConfirmation = 2,

    /// <summary>
    /// Indicates that the email verification code is for confirming the user's phone number.
    /// </summary>
    PhoneNumberConfirmation = 3,

    /// <summary>
    /// Indicates that the email verification code is for confirming a change to the user's phone number.
    /// </summary>
    PhoneNumberChangeConfirmation = 4,

    /// <summary>
    /// Indicates that the email verification code is for confirming two-factor authentication (2FA) setup or verification.
    /// </summary>
    TwoFactorAuthentication = 5

}