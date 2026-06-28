
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for authentication and session management.
/// Orchestrates credential validation, token generation, session lifecycle,
/// and the full e-mail verification / password-reset flows shown in the prototype.
/// </summary>
public interface IAuthService
{
    // ── Credential login ──────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user with e-mail and password.
    /// Creates a new <c>UserSession</c> and returns a token pair.
    /// Refresh-token lifetime is extended when <c>dto.RememberMe</c> is <c>true</c>.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the credentials are invalid or the account does not exist.
    /// </exception>
    Task<ApiResult<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? ipAddress);

    // ── Google OAuth ──────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates or auto-registers a user via a Google ID token.
    /// Creates the account if it does not yet exist, then issues a session.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the Google ID token is invalid or cannot be verified.
    /// </exception>
    Task<ApiResult<LoginResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto dto, string? ipAddress);

    // ── Token lifecycle ───────────────────────────────────────────────────

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair.
    /// Rotates the refresh token (old session revoked, new session created).
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the token is expired, revoked, or not found.
    /// </exception>
    Task<ApiResult<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto, string? ipAddress);

    /// <summary>
    /// Revokes the session associated with the given refresh token (logout).
    /// </summary>
    Task<ApiResult<EmptyDto>> LogoutAsync(string refreshToken);

    // ── Push-notification token ───────────────────────────────────────────

    /// <summary>
    /// Registers or clears the FCM / APNs push-notification token for a session.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the session is not found.</exception>
    Task<ApiResult<EmptyDto>> UpdatePushNotificationTokenAsync(
        Guid sessionId,
        UpdatePushNotificationTokenRequestDto dto
        );

    // ── E-mail verification ───────────────────────────────────────────────

    /// <summary>
    /// Validates the 6-digit code from the "Confirme seu email" screen.
    /// Marks the user's e-mail as verified on success.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the code is invalid, expired, or already used.
    /// </exception>
    Task<ApiResult<EmptyDto>> VerifyEmailAsync(VerifyEmailRequestDto dto, Guid loggedUserId);

    /// <summary>
    /// Re-sends the e-mail verification code after the 60-second cooldown.
    /// Invalidates any previously issued pending code before dispatching a new one.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when no account matches the given e-mail.</exception>
    Task<ApiResult<EmptyDto>> ResendVerificationEmailAsync(ResendVerificationEmailRequestDto dto, Guid loggedUserId);

    // ── Forgot password ───────────────────────────────────────────────────

    /// <summary>
    /// Initiates the password-reset flow ("Esqueci minha senha" — step 1).
    /// If an account with the given e-mail exists, a 6-digit reset code is sent.
    /// The response is always generic to prevent user enumeration.
    /// </summary>
    Task<ApiResult<EmptyDto>> ForgotPasswordAsync(ForgotPasswordRequestDto dto);

    /// <summary>
    /// Validates the 6-digit reset code ("Esqueci minha senha" — step 2).
    /// Returns a short-lived reset token that authorises the final password-set step.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the code is invalid, expired, or already used.
    /// </exception>
    Task<ApiResult<VerifyResetCodeResponseDto>> VerifyResetCodeAsync(VerifyResetCodeRequestDto dto);

    /// <summary>
    /// Sets a new password using the reset token obtained in step 2 ("Esqueci minha senha" — step 3).
    /// Revokes all active sessions on success.
    /// </summary>
    /// <exception cref="UnauthorizedAccessException">
    /// Thrown when the reset token is invalid or expired.
    /// </exception>
    Task<ApiResult<EmptyDto>> ResetPasswordAsync(ResetPasswordRequestDto dto);
}
