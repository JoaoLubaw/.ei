using System.Net;
using System.Text.Json;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Pontuei.Api.Common;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IUserSessionRepository _userSessionRepository;
    private readonly ITokenService _tokenService;
    private readonly IEmailService _emailService;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IDistributedCache _cache;
    private readonly ILogger<AuthService> _logger;

    public AuthService(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        IUserSessionRepository userSessionRepository,
        ITokenService tokenService,
        IEmailService emailService,
        IUnitOfWork unitOfWork,
        IDistributedCache cache,
        ILogger<AuthService> logger)
    {
        _userRepository = userRepository;
        _verificationCodeRepository = verificationCodeRepository;
        _userSessionRepository = userSessionRepository;
        _tokenService = tokenService;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Validates the 6-digit code from the "Confirme seu email" screen.
    /// Marks the user's e-mail as verified on success.
    /// </summary>
    public async Task<ApiResult<EmptyDto>> VerifyEmailAsync(VerifyEmailRequestDto dto, Guid loggedUserId)
    {
        User? user = await _userRepository.GetByIdAsync(loggedUserId);

        if (user == null)
        {
            _logger.LogWarning("Try to verify email for non-existent user: {UserId}", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (user.UserEmailVerified)
        {
            _logger.LogWarning("User {UserId} attempted to verify an already verified email.", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.EMAIL_ALREADY_VERIFIED,
                HttpStatusCode.BadRequest,
                null);
        }

        VerificationCode? pendingCode = await _verificationCodeRepository.GetPendingAsync(
                loggedUserId, VerificationCodeType.EmailConfirmation);

        if (pendingCode == null)
        {
            pendingCode = await _verificationCodeRepository.GetPendingAsync(
                loggedUserId, VerificationCodeType.EmailChangeConfirmation);
        }

        if (pendingCode == null)
        {
            _logger.LogWarning("Nenhum código pendente/válido encontrado para o usuário: {UserId}", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        string hashedInputCode = ValidationUtils.HashToken(dto.Code);

        if (pendingCode.VerificationCodeHash != hashedInputCode)
        {
            await _verificationCodeRepository.IncrementFailedAttemptsAsync(pendingCode, user.UserId.ToString());
            await _unitOfWork.CommitAsync();

            _logger.LogWarning("Código de verificação inválido inserido para o usuário: {UserId}", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.INVALID_CODE,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (pendingCode.VerificationCodeType == VerificationCodeType.EmailChangeConfirmation)
        {
            if (!string.IsNullOrEmpty(pendingCode.VerificationCodePayload))
            {
                user.UserEmail = pendingCode.VerificationCodePayload;
            }
        }

        await _verificationCodeRepository.MarkAsUsedAsync(pendingCode, user.UserId.ToString());
        await _userRepository.SetEmailVerifiedAsync(user, user.UserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();
        if (!saved)
        {
            _logger.LogError("Failed to save email verification in the database for user: {UserId}", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null);
        }

        _logger.LogInformation("Email verified successfully for user: {UserId}", loggedUserId);

        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

    public async Task<ApiResult<LoginResponseDto>> LoginAsync(LoginRequestDto dto, string? ipAddress)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.UserEmail);

        if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.UserPasswordHash))
        {
            _logger.LogWarning("Failed to login with email: {Email}", dto.UserEmail);

            return new ApiResult<LoginResponseDto>(
                InternalResultCode.INVALID_CREDENTIALS,
                HttpStatusCode.Unauthorized,
                null
            );
        }
        string accessToken = await _tokenService.GenerateAccessToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();

        string hashedRefreshToken = ValidationUtils.HashToken(refreshToken);

        DateTime expiration = dto.RememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(1);

        await _userSessionRepository.CreateAsync(
            userId: user.UserId,
            refreshTokenHash: hashedRefreshToken,
            refreshTokenExpiresAt: expiration,
            deviceInfo: dto.DeviceInfo,
            ipAddress: ipAddress,
            pushNotificationToken: null,
            createdBy: user.UserId.ToString()
        );

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to save login session for user: {UserId}", user.UserId);

            return new ApiResult<LoginResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );
        }

        _logger.LogInformation("Login successful for user: {UserId}. Remember me: {Remember}", user.UserId, dto.RememberMe);

        UserSessionData sessionData = new UserSessionData
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            DeviceInfo = dto.DeviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        string jsonSession = JsonSerializer.Serialize(sessionData);

        string redisKey = $"session:{user.UserId}:{hashedRefreshToken}";

        await _cache.SetStringAsync(
                redisKey,
                jsonSession,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration
                });

        return new ApiResult<LoginResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = expiration,
                User = user.Adapt<UserDto>()
            }
        );
    }
    public async Task<ApiResult<LoginResponseDto>> RefreshTokenAsync(RefreshTokenRequestDto dto, string? ipAddress)
    {
        string hashedInputToken = ValidationUtils.HashToken(dto.RefreshToken);

        UserSession? activeSession = await _userSessionRepository.GetActiveByRefreshTokenHashAsync(hashedInputToken);

        if (activeSession == null)
        {
            _logger.LogWarning("Attempt to use invalid or expired refresh token.");

            return new ApiResult<LoginResponseDto>(
                InternalResultCode.REFRESH_TOKEN_EXPIRED,
                HttpStatusCode.Unauthorized,
                null);
        }

        User? user = await _userRepository.GetByIdAsync(activeSession.UserId);

        if (user == null || user.IsDeleted)
        {
            return new ApiResult<LoginResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.Unauthorized,
                null);
        }

        await _userSessionRepository.RevokeAsync(activeSession, user.UserId.ToString());

        string newAccessToken = await _tokenService.GenerateAccessToken(user);
        string newRefreshToken = _tokenService.GenerateRefreshToken();
        string hashedNewRefreshToken = ValidationUtils.HashToken(newRefreshToken);

        DateTime expiration = DateTime.UtcNow.AddDays(30);

        _logger.LogInformation("Refresh token used for user: {UserId}. New session created.", user.UserId);

        await _userSessionRepository.CreateAsync(
            user.UserId,
            hashedNewRefreshToken,
            expiration,
            activeSession.UserSessionDeviceInfo,
            ipAddress,
            activeSession.UserSessionPushNotificationToken,
            user.UserId.ToString()
        );

        await _unitOfWork.CommitAsync();

        return new ApiResult<LoginResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiresAt = expiration,
                User = user.Adapt<UserDto>()
            }
        );
    }

    public async Task<ApiResult<EmptyDto>> LogoutAsync(string refreshToken)
    {
        string hashedInputToken = ValidationUtils.HashToken(refreshToken);
        UserSession? activeSession = await _userSessionRepository.GetActiveByRefreshTokenHashAsync(hashedInputToken);

        if (activeSession != null)
        {
            await _userSessionRepository.RevokeAsync(activeSession, activeSession.UserId.ToString());
            await _unitOfWork.CommitAsync();

            await _cache.RemoveAsync($"session:{activeSession.UserId}:{hashedInputToken}");
        }

        _logger.LogInformation("Logout successful for session: {SessionId}", activeSession?.UserSessionId);

        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

    public async Task<ApiResult<EmptyDto>> UpdatePushNotificationTokenAsync(Guid sessionId, UpdatePushNotificationTokenRequestDto dto)
    {
        UserSession? session = await _userSessionRepository.GetByIdAsync(sessionId);

        if (session == null)
        {
            return new ApiResult<EmptyDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        _logger.LogInformation("Updating push notification token for session: {SessionId}", sessionId);

        await _userSessionRepository.SetPushNotificationTokenAsync(session, dto.PushNotificationToken, session.UserId.ToString());
        await _unitOfWork.CommitAsync();

        return new ApiResult<EmptyDto>(InternalResultCode.NO_ERROR, HttpStatusCode.OK, null);
    }

    public async Task<ApiResult<EmptyDto>> ResendVerificationEmailAsync(ResendVerificationEmailRequestDto dto, Guid loggedUserId)
    {
        User? user = await _userRepository.GetByIdAsync(loggedUserId);

        if (user == null)
        {
            _logger.LogWarning("Attempt to resend verification email for non-existent user: {UserId}", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (user.UserEmailVerified)
        {
            _logger.LogWarning("User {UserId} attempted to resend verification email for an already verified email.", loggedUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.EMAIL_ALREADY_VERIFIED,
                HttpStatusCode.BadRequest,
                null
            );
        }

        await _verificationCodeRepository.InvalidatePendingAsync(user.UserId, VerificationCodeType.EmailConfirmation, user.UserId.ToString());

        string newCode = ValidationUtils.Generate6DigitCode();
        string hashedCode = ValidationUtils.HashToken(newCode);

        await _verificationCodeRepository.CreateAsync(
            userId: user.UserId,
            type: VerificationCodeType.EmailConfirmation,
            code: hashedCode,
            payload: null,
            expiresAt: DateTime.UtcNow.AddMinutes(15),
            createdBy: user.UserId.ToString()
        );

        await _unitOfWork.CommitAsync();

        await _emailService.SendVerificationEmailAsync(user.UserEmail, user.UserName, newCode);

        _logger.LogInformation("Verification email resent for user: {UserId}", user.UserId);

        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

    public async Task<ApiResult<EmptyDto>> ForgotPasswordAsync(ForgotPasswordRequestDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.UserEmail);

        if (user != null)
        {
            await _verificationCodeRepository.InvalidatePendingAsync(user.UserId, VerificationCodeType.PasswordReset, "System");

            string resetCode = ValidationUtils.Generate6DigitCode();
            string hashedCode = ValidationUtils.HashToken(resetCode);

            await _verificationCodeRepository.CreateAsync(
                userId: user.UserId,
                type: VerificationCodeType.PasswordReset,
                code: hashedCode,
                payload: null,
                expiresAt: DateTime.UtcNow.AddMinutes(15),
                createdBy: "System"
            );

            await _unitOfWork.CommitAsync();

            _logger.LogInformation("Reset password token sent for user: {UserId}", user.UserId);

            await _emailService.SendResetPasswordToken(user.UserEmail, user.UserName, resetCode);
        }

        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

    public async Task<ApiResult<VerifyResetCodeResponseDto>> VerifyResetCodeAsync(VerifyResetCodeRequestDto dto)
    {
        User? user = await _userRepository.GetByEmailAsync(dto.UserEmail);

        if (user == null)

            return new ApiResult<VerifyResetCodeResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );

        VerificationCode? pendingCode = await _verificationCodeRepository.GetPendingAsync(user.UserId, VerificationCodeType.PasswordReset);

        if (pendingCode == null)

            return new ApiResult<VerifyResetCodeResponseDto>(
            InternalResultCode.ENTITY_NOT_FOUND,
            HttpStatusCode.NotFound,
            null
        );

        string hashedInputCode = ValidationUtils.HashToken(dto.Code);

        if (pendingCode.VerificationCodeHash != hashedInputCode)
        {
            await _verificationCodeRepository.IncrementFailedAttemptsAsync(pendingCode, dto.UserEmail);
            await _unitOfWork.CommitAsync();

            return new ApiResult<VerifyResetCodeResponseDto>(
                InternalResultCode.INVALID_CODE,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        _logger.LogInformation("Password reset code verified for user: {UserId}", user.UserId);

        await _verificationCodeRepository.MarkAsUsedAsync(pendingCode, "PasswordReset");

        string resetToken = Guid.NewGuid().ToString("N");
        string hashedResetToken = ValidationUtils.HashToken(resetToken);

        await _verificationCodeRepository.CreateAsync(
            userId: user.UserId,
            type: VerificationCodeType.PasswordReset,
            code: hashedResetToken,
            payload: "AUTHORIZED_RESET",
            expiresAt: DateTime.UtcNow.AddMinutes(15),
            createdBy: dto.UserEmail
        );

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("Temporary reset token generated for user: {UserId}", user.UserId);

        return new ApiResult<VerifyResetCodeResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new VerifyResetCodeResponseDto { ResetToken = resetToken });
    }

    public async Task<ApiResult<EmptyDto>> ResetPasswordAsync(ResetPasswordRequestDto dto)
    {
        if (!dto.IsValidPassword())
        {
            return new ApiResult<EmptyDto>(
                InternalResultCode.INVALID_USER_PASSWORD,
                HttpStatusCode.BadRequest,
                null
            );
        }

        if (!dto.PasswordsMatch())
        {
            return new ApiResult<EmptyDto>(
                InternalResultCode.PASSWORDS_DO_NOT_MATCH,
                HttpStatusCode.BadRequest,
                null
            );
        }

        string hashedInputToken = ValidationUtils.HashToken(dto.ResetToken);

        VerificationCode? validationContext = await _verificationCodeRepository.GetByHashAsync(
            hashedInputToken,
            VerificationCodeType.PasswordReset,
            verifyActive: true
        );

        if (validationContext == null)
        {
            return new ApiResult<EmptyDto>(
                InternalResultCode.INVALID_CODE,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        User? userToUpdate = await _userRepository.GetByIdAsync(validationContext.UserId);

        string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        await _userRepository.UpdatePasswordAsync(userToUpdate!, newHashedPassword, "PasswordReset");

        await _verificationCodeRepository.MarkAsUsedAsync(validationContext, "PasswordReset");

        List<UserSession> activeSessions = await _userSessionRepository.GetActiveByUserIdAsync(userToUpdate!.UserId).ToListAsync();

        await _userSessionRepository.RevokeAllByUserIdAsync(userToUpdate, "PasswordReset");
        await _unitOfWork.CommitAsync();

        foreach (UserSession session in activeSessions)
        {
            string redisKey = $"session:{userToUpdate.UserId}:{session.UserSessionRefreshToken}";
            await _cache.RemoveAsync(redisKey);
        }

        _logger.LogInformation("Password reset successful and all sessions revoked for user: {UserId}", userToUpdate.UserId);
        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

    /// <summary>
    /// Handles Google OAuth login. Validates the provided Google ID Token, retrieves or creates a user, and returns access and refresh tokens.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="ipAddress"></param>
    /// <returns></returns>
    public async Task<ApiResult<LoginResponseDto>> GoogleLoginAsync(GoogleLoginRequestDto dto, string? ipAddress)
    {
        FirebaseAdmin.Auth.FirebaseToken decodedToken;

        try
        {
            decodedToken = await FirebaseAdmin.Auth.FirebaseAuth.DefaultInstance.VerifyIdTokenAsync(dto.IdToken);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to verify Google ID token.");

            return new ApiResult<LoginResponseDto>(
                InternalResultCode.INVALID_CREDENTIALS,
                HttpStatusCode.Unauthorized,
                null);
        }

        string googleId = decodedToken.Uid;
        string? email = decodedToken.Claims.GetValueOrDefault("email")?.ToString();
        string? name = decodedToken.Claims.GetValueOrDefault("name")?.ToString();

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(name))
        {
            _logger.LogWarning("Google ID token is missing required claims: email or name.");

            return new ApiResult<LoginResponseDto>(
                InternalResultCode.INVALID_CREDENTIALS,
                HttpStatusCode.NotFound,
                null);
        }

        User? user = await _userRepository.GetByGoogleIdAsync(googleId);

        if (user == null)
        {
            user = await _userRepository.GetByEmailAsync(email);

            if (user != null)
            {
                user.UserGoogleId = googleId;

                _logger.LogInformation("Existing user found by email. Linked Google ID: {GoogleId} to user: {UserId}", googleId, user.UserId);
            }
            else
            {
                CreateUserRequestDto autoCreateDto = new CreateUserRequestDto
                {
                    UserName = name,
                    UserEmail = email,
                    Password = Guid.NewGuid().ToString(),
                    ConfirmPassword = string.Empty,
                    UserAcceptedTerms = true,
                    UserPushNotificationsEnabled = true,
                    UserEmailNotificationsEnabled = true
                };

                user = await _userRepository.CreateAsync(autoCreateDto, BCrypt.Net.BCrypt.HashPassword(autoCreateDto.Password), "Google-OAuth");
                user.UserGoogleId = googleId;

                await _userRepository.SetEmailVerifiedAsync(user, "Google-OAuth");
            }
        }

        string accessToken = await _tokenService.GenerateAccessToken(user);
        string refreshToken = _tokenService.GenerateRefreshToken();
        string hashedRefreshToken = ValidationUtils.HashToken(refreshToken);

        DateTime expiration = DateTime.UtcNow.AddDays(30);

        await _userSessionRepository.CreateAsync(
            userId: user.UserId,
            refreshTokenHash: hashedRefreshToken,
            refreshTokenExpiresAt: expiration,
            deviceInfo: dto.DeviceInfo,
            ipAddress: ipAddress,
            pushNotificationToken: null,
            createdBy: user.UserId.ToString()
        );

        await _unitOfWork.CommitAsync();

        _logger.LogInformation("User logged in successfully: {UserId}", user.UserId);

        UserSessionData sessionData = new UserSessionData
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            DeviceInfo = dto.DeviceInfo,
            CreatedAt = DateTime.UtcNow
        };

        string jsonSession = JsonSerializer.Serialize(sessionData);

        string redisKey = $"session:{user.UserId}:{hashedRefreshToken}";

        await _cache.SetStringAsync(
                redisKey,
                jsonSession,
                new DistributedCacheEntryOptions
                {
                    AbsoluteExpiration = expiration
                });

        return new ApiResult<LoginResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                RefreshTokenExpiresAt = expiration,
                User = user.Adapt<UserDto>()
            }
        );
    }
}