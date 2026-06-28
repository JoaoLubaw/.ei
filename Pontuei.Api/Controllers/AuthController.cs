using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Api.Interfaces.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace Pontuei.Api.Controllers;

[Route("auth")]
[SwaggerTag("Authentication and session management")]
public class AuthController : PontueiControllerBase
{
    private readonly IAuthService _authService;
    private readonly IUserService _userService;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        ILogger<AuthController> logger) : base(logger)
    {
        _authService = authService;
        _userService = userService;
    }

    // ── Credential login ──────────────────────────────────────────────────

    /// <summary>
    /// Authenticates a user with e-mail and password, returning a token pair.
    /// </summary>
    /// <remarks>
    /// Creates a new session. Pass <c>rememberMe: true</c> to extend the refresh-token lifetime.
    /// </remarks>
    /// <response code="200">Authentication succeeded. Returns access and refresh tokens.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Invalid credentials or account not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("login")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Login([FromBody] LoginRequestDto requestDto)
    {
        _logger.LogInformation("Login attempt for e-mail: {Email}", requestDto.UserEmail);

        try
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            ApiResult<LoginResponseDto> apiResult = await _authService.LoginAsync(requestDto, ipAddress);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(Login));
        }
    }

    // ── Google OAuth ──────────────────────────────────────────────────────

    /// <summary>
    /// Authenticates or auto-registers a user via a Google ID token.
    /// </summary>
    /// <remarks>
    /// If the account does not exist it is created automatically before the session is issued.
    /// </remarks>
    /// <response code="200">Authentication succeeded. Returns access and refresh tokens.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Google ID token is invalid or could not be verified.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("login/google")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> GoogleLogin([FromBody] GoogleLoginRequestDto requestDto)
    {
        _logger.LogInformation("Google login attempt");

        try
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            ApiResult<LoginResponseDto> apiResult = await _authService.GoogleLoginAsync(requestDto, ipAddress);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(GoogleLogin));
        }
    }

    // ── Registration with auto-login ──────────────────────────────────────

    /// <summary>
    /// Registers a new user account and immediately issues a session (auto-login).
    /// </summary>
    /// <remarks>
    /// Validates e-mail uniqueness, hashes the password, persists the user, dispatches
    /// the e-mail verification code, and then performs an automatic login so the app
    /// receives a ready-to-use token pair in the same response.
    /// The device info is extracted from the <c>User-Agent</c> header.
    /// </remarks>
    /// <response code="200">Registration and auto-login succeeded. Returns access and refresh tokens.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="409">E-mail is already registered.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status409Conflict, Type = typeof(ErrorApiResult))]
    [HttpPost("register")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> Register([FromBody] CreateUserRequestDto requestDto)
    {
        _logger.LogInformation("Registration attempt for e-mail: {Email}", requestDto.UserEmail);

        try
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();

            // 1. Create the account
            ApiResult<UserDto> registerResult = await _userService.RegisterAsync(requestDto, null);
            if (registerResult.HttpCode != System.Net.HttpStatusCode.OK &&
                registerResult.HttpCode != System.Net.HttpStatusCode.Created)
            {
                return ToActionResult(registerResult);
            }

            // 2. Auto-login — produce the token pair right away so the app can start navigating
            LoginRequestDto loginDto = new LoginRequestDto
            {
                UserEmail = requestDto.UserEmail,
                Password = requestDto.Password,
                RememberMe = false,
                DeviceInfo = HttpContext.Request.Headers.UserAgent.ToString()
            };

            ApiResult<LoginResponseDto> loginResult = await _authService.LoginAsync(loginDto, ipAddress);
            return ToActionResult(loginResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(Register));
        }
    }

    // ── Token lifecycle ───────────────────────────────────────────────────

    /// <summary>
    /// Exchanges a valid refresh token for a new access/refresh token pair.
    /// </summary>
    /// <remarks>
    /// Rotates the refresh token — the old session is revoked and a new one is created.
    /// </remarks>
    /// <response code="200">Token refreshed successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Token is expired, revoked, or not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(LoginResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("refresh")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult<LoginResponseDto>> RefreshToken([FromBody] RefreshTokenRequestDto requestDto)
    {
        try
        {
            string? ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString();
            ApiResult<LoginResponseDto> apiResult = await _authService.RefreshTokenAsync(requestDto, ipAddress);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(RefreshToken));
        }
    }

    /// <summary>
    /// Revokes the session associated with the provided refresh token (logout).
    /// </summary>
    /// <response code="204">Session revoked successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("logout")]
    [Authorize]
    public async Task<ActionResult> Logout([FromBody] string refreshToken)
    {
        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.LogoutAsync(refreshToken);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(Logout));
        }
    }

    // ── Push-notification token ───────────────────────────────────────────

    /// <summary>
    /// Registers or clears the FCM / APNs push-notification token for a session.
    /// </summary>
    /// <response code="204">Push token updated successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Requires session authentication.</response>
    /// <response code="404">Session not found.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPut("sessions/{sessionId}/push-token")]
    [EnableRateLimiting("StrictAuthLimit")]
    [Authorize]
    public async Task<ActionResult> UpdatePushNotificationToken(
        [FromRoute] Guid sessionId,
        [FromBody] UpdatePushNotificationTokenRequestDto requestDto)
    {
        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.UpdatePushNotificationTokenAsync(sessionId, requestDto);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(UpdatePushNotificationToken));
        }
    }

    // ── E-mail verification ───────────────────────────────────────────────

    /// <summary>
    /// Validates the 6-digit code from the "Confirme seu email" screen.
    /// </summary>
    /// <remarks>
    /// Marks the user's e-mail as verified on success.
    /// </remarks>
    /// <response code="204">E-mail verified successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Code is invalid, expired, or already used.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("verify-email")]
    [EnableRateLimiting("StrictAuthLimit")]
    [Authorize]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.VerifyEmailAsync(requestDto, currentUserId.Value);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(VerifyEmail));
        }
    }

    /// <summary>
    /// Re-sends the e-mail verification code after the 60-second cooldown.
    /// </summary>
    /// <remarks>
    /// Invalidates any previously issued pending code before dispatching a new one.
    /// </remarks>
    /// <response code="204">Verification e-mail dispatched successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="404">No account matches the given e-mail.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPost("resend-verification")]
    [EnableRateLimiting("StrictAuthLimit")]
    [Authorize]
    public async Task<ActionResult> ResendVerificationEmail([FromBody] ResendVerificationEmailRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.ResendVerificationEmailAsync(requestDto, currentUserId.Value);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(ResendVerificationEmail));
        }
    }

    // ── Forgot password ───────────────────────────────────────────────────

    /// <summary>
    /// Initiates the password-reset flow — step 1 of 3.
    /// </summary>
    /// <remarks>
    /// If an account with the given e-mail exists, a 6-digit reset code is sent.
    /// The response is always generic to prevent user enumeration.
    /// </remarks>
    /// <response code="204">Request processed (response is intentionally generic).</response>
    /// <response code="400">Bad arguments passed.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [HttpPost("forgot-password")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult> ForgotPassword([FromBody] ForgotPasswordRequestDto requestDto)
    {
        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.ForgotPasswordAsync(requestDto);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(ForgotPassword));
        }
    }

    /// <summary>
    /// Validates the 6-digit reset code — step 2 of 3.
    /// </summary>
    /// <remarks>
    /// Returns a short-lived reset token that authorises the final password-set step.
    /// </remarks>
    /// <response code="200">Code is valid. Returns a short-lived reset token.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Code is invalid, expired, or already used.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(VerifyResetCodeResponseDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("verify-reset-code")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult<VerifyResetCodeResponseDto>> VerifyResetCode([FromBody] VerifyResetCodeRequestDto requestDto)
    {
        try
        {
            ApiResult<VerifyResetCodeResponseDto> apiResult = await _authService.VerifyResetCodeAsync(requestDto);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(VerifyResetCode));
        }
    }

    /// <summary>
    /// Sets a new password using the reset token obtained in step 2 — step 3 of 3.
    /// </summary>
    /// <remarks>
    /// Revokes all active sessions on success.
    /// </remarks>
    /// <response code="204">Password reset successfully. All sessions revoked.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Reset token is invalid or expired.</response>
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [HttpPost("reset-password")]
    [EnableRateLimiting("StrictAuthLimit")]
    [AllowAnonymous]
    public async Task<ActionResult> ResetPassword([FromBody] ResetPasswordRequestDto requestDto)
    {
        try
        {
            ApiResult<EmptyDto> apiResult = await _authService.ResetPasswordAsync(requestDto);
            return ToNoContentResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(ResetPassword));
        }
    }

    // ── Password change (authenticated) ──────────────────────────────────

    /// <summary>
    /// Changes the authenticated user's password after verifying the current one.
    /// </summary>
    /// <remarks>
    /// Invalidates all existing sessions on success.
    /// </remarks>
    /// <response code="200">Password changed successfully.</response>
    /// <response code="400">Bad arguments passed.</response>
    /// <response code="401">Current password does not match or requires session authentication.</response>
    /// <response code="404">User not found.</response>
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(UserDto))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ErrorApiResult))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ErrorApiResult))]
    [HttpPost("change-password")]
    [EnableRateLimiting("StrictAuthLimit")]
    [Authorize]
    public async Task<ActionResult<UserDto>> ChangePassword([FromBody] ChangePasswordRequestDto requestDto)
    {
        Guid? currentUserId = CurrentUserId();
        if (currentUserId is null) return Unauthorized();

        try
        {
            ApiResult<UserDto> apiResult = await _userService.ChangePasswordAsync(currentUserId.Value, requestDto, currentUserId.Value);
            return ToActionResult(apiResult);
        }
        catch (Exception ex)
        {
            return HandleException(ex, nameof(ChangePassword));
        }
    }
}