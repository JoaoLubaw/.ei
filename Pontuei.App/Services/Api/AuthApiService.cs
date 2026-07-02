using Pontuei.App.Services.Api;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;

namespace Pontuei.App.Services;

/// <summary>
/// Covers all authentication-related API endpoints:
///   POST /auth/login
///   POST /auth/login/google
///   POST /auth/register
///   POST /auth/refresh
///   POST /auth/logout
///   POST /auth/verify-email
///   POST /auth/resend-verification
///   POST /auth/forgot-password
///   POST /auth/verify-reset-code
///   POST /auth/reset-password
///   POST /auth/change-password
/// </summary>
public class AuthApiService
{
    private readonly ApiClient _api;

    public AuthApiService(ApiClient api)
    {
        _api = api;
    }

    // ── Credential login ──────────────────────────────────────────────────

    /// <summary>
    /// Authenticates with email and password. In case of success, persists the tokens via AuthService.
    /// </summary>
    public async Task<ApiResponse<LoginResponseDto>> LoginAsync(LoginRequestDto request)
    {
        ApiResponse<LoginResponseDto> response = await _api.PostAsync<LoginResponseDto>("auth/login", request, true);

        if (response.IsSuccess && response.Data != null)
            await AuthService.SaveSessionAsync(response.Data);

        return response;
    }

    // ── Google OAuth ──────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates or registers the user via Google ID token.
    /// </summary>
    public async Task<ApiResponse<LoginResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto request)
    {
        ApiResponse<LoginResponseDto> response = await _api.PostAsync<LoginResponseDto>("auth/login/google", request);

        if (response.IsSuccess && response.Data != null)
            await AuthService.SaveSessionAsync(response.Data);

        return response;
    }

    // ── Auto-login register ───────────────────────────────────────────

    /// <summary>
    /// Creates the account and logs in automatically, returning the token pair.
    /// </summary>
    public async Task<ApiResponse<LoginResponseDto>> RegisterAsync(CreateUserRequestDto request)
    {
        ApiResponse<LoginResponseDto> response = await _api.PostAsync<LoginResponseDto>("auth/register", request, true);

        if (response.IsSuccess && response.Data != null)
            await AuthService.SaveSessionAsync(response.Data);

        return response;
    }

    // ── Push notification token ─────────────────────────────────────────
    public async Task<ApiResponse<EmptyDto>> UpdatePushTokenAsync(UpdatePushNotificationTokenRequestDto request)
    {
        Guid? sessionId = AuthService.CurrentSessionId;
        if (sessionId is null)
            return ApiResponse<EmptyDto>.Fail(System.Net.HttpStatusCode.BadRequest, "Sessão não encontrada.");

        return await _api.PatchAsync<EmptyDto>($"sessions/{sessionId}/push-token", request);
    }

    // ── Token lifecycle ───────────────────────────────────────────────────

    /// <summary>
    /// Exchanges the refresh token for a new pair of tokens (rotates the session).
    /// </summary>
    public async Task<ApiResponse<LoginResponseDto>> RefreshTokenAsync()
    {
        string? refreshToken = AuthService.RefreshToken;
        if (string.IsNullOrEmpty(refreshToken))
            return ApiResponse<LoginResponseDto>.Fail(System.Net.HttpStatusCode.Unauthorized, "Nenhum refresh token disponível.");

        RefreshTokenRequestDto request = new() { RefreshToken = refreshToken };
        ApiResponse<LoginResponseDto> response = await _api.PostAsync<LoginResponseDto>("auth/refresh", request, true);

        if (response.IsSuccess && response.Data != null)
            await AuthService.SaveSessionAsync(response.Data);

        return response;
    }

    /// <summary>
    /// Revokes the current session (logout).
    /// </summary>
    public async Task<ApiResponse<EmptyDto>> LogoutAsync()
    {
        string? refreshToken = AuthService.RefreshToken;

        await AuthService.LogoutAsync();

        if (string.IsNullOrEmpty(refreshToken))
            return ApiResponse<EmptyDto>.Ok(new EmptyDto());

        var body = new { refreshToken };
        return await _api.PostAsync<EmptyDto>("auth/logout", body);
    }

    // ── E-mail verification ─────────────────────────────────────────────

    /// <summary>
    /// Verifies the user's email using the 6-digit code sent to their inbox. If successful, the user's email is marked as verified.
    /// </summary>
    public Task<ApiResponse<EmptyDto>> VerifyEmailAsync(VerifyEmailRequestDto request)
        => _api.PostAsync<EmptyDto>("auth/verify-email", request, true);

    /// <summary>
    /// Resends the verification code to the user's email. The response is always generic to prevent user enumeration.
    /// </summary>
    public Task<ApiResponse<EmptyDto>> ResendVerificationEmailAsync(ResendVerificationEmailRequestDto request)
        => _api.PostAsync<EmptyDto>("auth/resend-verification", request, true);

    // ── Forgot password (3 steps) ────────────────────────────────────────

    /// <summary>
    /// Step 1 — requests the sending of the reset code via email.
    /// The response is always generic to prevent user enumeration.
    /// </summary>
    public Task<ApiResponse<EmptyDto>> ForgotPasswordAsync(ForgotPasswordRequestDto request)
        => _api.PostAsync<EmptyDto>("auth/forgot-password", request, true);

    /// <summary>
    /// Step 2 — validates the 6-digit code and returns a short-lived reset token.
    /// </summary>
    public Task<ApiResponse<VerifyResetCodeResponseDto>> VerifyResetCodeAsync(VerifyResetCodeRequestDto request)
        => _api.PostAsync<VerifyResetCodeResponseDto>("auth/verify-reset-code", request, true);

    /// <summary>
    /// Step 3 — defines the new password using the token obtained in step 2.
    /// Revokes all active sessions upon completion.
    /// </summary>
    public Task<ApiResponse<EmptyDto>> ResetPasswordAsync(ResetPasswordRequestDto request)
        => _api.PostAsync<EmptyDto>("auth/reset-password", request);

    // ── Change password (authenticated) ────────────────────────────────────

    /// <summary>
    /// Changes the password of the currently authenticated user. Requires the current password for confirmation. 
    /// Revokes all active sessions upon completion.
    /// </summary>
    public Task<ApiResponse<UserDto>> ChangePasswordAsync(ChangePasswordRequestDto request)
        => _api.PostAsync<UserDto>("auth/change-password", request, true);
}