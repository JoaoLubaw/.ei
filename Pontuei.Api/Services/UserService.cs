using System.Net;
using System.Security.Cryptography;
using System.Text;
using BCrypt.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Common;

using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IVerificationCodeRepository _verificationCodeRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IEmailService _emailService;
    private readonly ITokenService _tokenService;
    private readonly ILogger<UserService> _logger;

    public UserService(
        IUserRepository userRepository,
        IVerificationCodeRepository verificationCodeRepository,
        IUnitOfWork unitOfWork,
        IEmailService emailService,
        ITokenService tokenService,
        ILogger<UserService> logger)
    {
        _userRepository = userRepository;
        _verificationCodeRepository = verificationCodeRepository;
        _unitOfWork = unitOfWork;
        _emailService = emailService;
        _tokenService = tokenService;
        _logger = logger;
    }

    /// <summary>
    /// Returns a paginated list of users matching the given filter criteria.
    /// </summary>
    /// <param name="dto"></param>
    /// <param name="currentUserId"></param>
    /// <returns></returns>
    public async Task<ApiResult<GetUsersResponseDto>> GetUsers(GetUsersRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<GetUsersResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (!loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User with ID {UserId} attempted to access user list without admin privileges. Returned only their own information.", currentUserId);

            return new ApiResult<GetUsersResponseDto>(
                InternalResultCode.NO_ERROR,
                HttpStatusCode.OK,
                new GetUsersResponseDto
                {
                    TotalElements = 1,
                    TotalPages = 1,
                    Users = new List<UserDto>
                    {
                        loggedUser.Adapt<UserDto>()
                    }
                }
            );
        }

        IQueryable<User> query = _userRepository.GetAllUsers();
        query = ApplyFilters(query, dto);

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<User> users = await query
            .OrderBy(u => u.UserName)
            .Skip(skip)
            .Take(dto.Size)
            .ToListAsync();

        return new ApiResult<GetUsersResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetUsersResponseDto
            {
                TotalElements = totalElements,
                TotalPages = totalPages,
                Users = users.Adapt<List<UserDto>>()
            }
        );
    }

    IQueryable<User> ApplyFilters(IQueryable<User> query, GetUsersRequestDto dto)
    {
        if (dto.Filters != null)
        {
            if (dto.Filters.UserId.HasValue)
            {
                query = query.Where(u => u.UserId == dto.Filters.UserId.Value);
            }

            if (!string.IsNullOrEmpty(dto.Filters.UserEmail))
            {
                query = query.Where(p => p.UserEmail != null &&
                    EF.Functions.ILike(p.UserEmail, dto.Filters.UserEmail + "%"));
            }

            if (!string.IsNullOrEmpty(dto.Filters.UserName))
            {
                query = query.Where(p => p.UserName != null &&
                    EF.Functions.ILike(p.UserName, dto.Filters.UserName + "%"));
            }

            if (!string.IsNullOrEmpty(dto.Filters.UserPhoneNumber))
            {
                query = query.Where(p => p.UserPhoneNumber != null &&
                    EF.Functions.ILike(p.UserPhoneNumber, dto.Filters.UserPhoneNumber + "%"));
            }

            if (dto.Filters.UserIsAdmin.HasValue)
            {
                query = query.Where(u => u.UserIsAdmin == dto.Filters.UserIsAdmin.Value);
            }

            if (dto.Filters.UserEmailVerified.HasValue)
            {
                query = query.Where(u => u.UserEmailVerified == dto.Filters.UserEmailVerified.Value);
            }
        }

        return query;
    }

    /// <summary>
    /// Returns a user summary by ID, or <c>null</c> when not found.
    /// </summary>
    public async Task<ApiResult<UserDto>> GetByIdAsync(Guid userId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<UserDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        User? user = await _userRepository.GetByIdAsync(userId);

        if (user == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);

            return new ApiResult<UserDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        VerificationCode? pendingCode = await _verificationCodeRepository.GetPendingAsync(
            user.UserId,
            VerificationCodeType.EmailChangeConfirmation
        );

        UserDto userDto = user.Adapt<UserDto>();

        if (pendingCode != null)
        {
            userDto.PendingEmail = pendingCode.VerificationCodePayload;
        }

        if (!loggedUser.UserIsAdmin && loggedUser.UserId != userId)
        {
            _logger.LogWarning("User with ID {UserId} attempted to access user {TargetUserId} without permission.", currentUserId, userId);

            return new ApiResult<UserDto>(
                InternalResultCode.NOT_ALLOWED_TO_GET_THIS_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        return new ApiResult<UserDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            userDto
        );

    }

    /// <summary>
    /// Registers a new user account from the "Nova conta" screen.
    /// Validates uniqueness, hashes the password, persists the user,
    /// and dispatches the e-mail verification code.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the e-mail is already registered.</exception>
    public async Task<ApiResult<UserDto>> RegisterAsync(CreateUserRequestDto dto, Guid? currentUserId)
    {
        if (currentUserId != null)
        {
            User? loggedUser = await _userRepository.GetByIdAsync(currentUserId.Value);

            if (loggedUser == null || !loggedUser.UserIsAdmin)
            {
                _logger.LogWarning("User with ID {UserId} attempted to register a new user without admin privileges.", currentUserId);

                return new ApiResult<UserDto>(
                    InternalResultCode.NOT_ADMIN,
                    HttpStatusCode.Forbidden,
                    null
                );
            }
        }

        if (!dto.IsValid())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.MISSING_INFORMATION,
                HttpStatusCode.BadRequest,
                null);
        }

        if (!dto.IsValidEmail())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.INVALID_USER_EMAIL,
                HttpStatusCode.BadRequest,
                null);
        }

        if (!dto.IsValidPassword())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.INVALID_USER_PASSWORD,
                HttpStatusCode.BadRequest,
                null);
        }

        if (!dto.PasswordsMatch())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.PASSWORDS_DO_NOT_MATCH,
                HttpStatusCode.BadRequest,
                null);
        }

        if (dto.UserIsAdmin == true && currentUserId == null)
        {
            _logger.LogWarning("Attempt to register an admin user without being logged in.");

            return new ApiResult<UserDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null
            );
        }

        User? existingUser = await _userRepository.GetByEmailAsync(dto.UserEmail, true);

        if (existingUser != null)
        {
            _logger.LogWarning("Attempt to register existing email: {Email}", dto.UserEmail);

            return new ApiResult<UserDto>(
                InternalResultCode.EMAIL_ALREADY_TAKEN,
                HttpStatusCode.Conflict,
                null);
        }

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password);

        User newUser = await _userRepository.CreateAsync(dto, passwordHash, currentUserId?.ToString() ?? "Own-Registration");

        string confirmationToken = ValidationUtils.Generate6DigitCode();
        string hashedConfirmationToken = ValidationUtils.HashToken(confirmationToken);

        await _verificationCodeRepository.CreateAsync(
            userId: newUser.UserId,
            type: VerificationCodeType.EmailConfirmation,
            code: hashedConfirmationToken,
            payload: null,
            expiresAt: DateTime.UtcNow.AddMinutes(15),
            createdBy: currentUserId?.ToString() ?? newUser.UserId.ToString()
        );

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to commit transaction for user: {Email}", dto.UserEmail);

            return new ApiResult<UserDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null
            );
        }

        await _emailService.SendVerificationEmailAsync(newUser.UserEmail, newUser.UserName, confirmationToken);

        return new ApiResult<UserDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.Created,
            newUser.Adapt<UserDto>()
        );

    }

    /// <summary>
    /// Applies partial updates from the "Informações de conta" settings screen.
    /// When <c>userEmail</c> changes, a new verification flow is triggered.
    /// Returns the updated summary.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="InvalidOperationException">Thrown when the new e-mail is already taken.</exception>
    public async Task<ApiResult<UserDto>> UpdateAsync(Guid userId, UpdateUserRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<UserDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        User? userToUpdate = await _userRepository.GetByIdAsync(userId);

        if (userToUpdate == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);

            return new ApiResult<UserDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (!loggedUser.UserIsAdmin && loggedUser.UserId != userId)
        {
            _logger.LogWarning("User with ID {UserId} attempted to update user {TargetUserId} without permission.", currentUserId, userId);

            return new ApiResult<UserDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        if (dto.UserEmail != null)
        {
            if (!dto.IsValidEmail())
            {
                return new ApiResult<UserDto>(
                    InternalResultCode.INVALID_USER_EMAIL,
                    HttpStatusCode.BadRequest,
                    null);
            }

            if (await _userRepository.GetByEmailAsync(dto.UserEmail, true) != null)
            {
                return new ApiResult<UserDto>(
                    InternalResultCode.EMAIL_ALREADY_TAKEN,
                    HttpStatusCode.Conflict,
                    null);
            }

            string confirmationToken = ValidationUtils.Generate6DigitCode();
            string hashedConfirmationToken = ValidationUtils.HashToken(confirmationToken);

            await _verificationCodeRepository.CreateAsync(
                userId: userToUpdate.UserId,
                type: VerificationCodeType.EmailChangeConfirmation,
                code: hashedConfirmationToken,
                payload: dto.UserEmail,
                expiresAt: DateTime.UtcNow.AddMinutes(15),
                createdBy: currentUserId.ToString() ?? currentUserId!.ToString()
            );
        }

        if (dto.UserPhoneNumber != null && !dto.IsValidPhoneNumber())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.INVALID_USER_PHONE_NUMBER,
                HttpStatusCode.BadRequest,
                null);
        }

        _logger.LogInformation("Updating user {UserId} with data: {@UpdateData}", userId, dto);

        User updatedUser = await _userRepository.UpdateAsync(userToUpdate, dto, loggedUser.UserName);

        await _unitOfWork.CommitAsync();

        return new ApiResult<UserDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            updatedUser.Adapt<UserDto>()
        );
    }

    /// <summary>
    /// Changes the authenticated user's password after verifying the current one.
    /// Invalidates all existing sessions on success.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    /// <exception cref="UnauthorizedAccessException">Thrown when <c>currentPassword</c> does not match.</exception>
    public async Task<ApiResult<UserDto>> ChangePasswordAsync(Guid userId, ChangePasswordRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<UserDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        User? userToUpdate = await _userRepository.GetByIdAsync(userId);

        if (userToUpdate == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);

            return new ApiResult<UserDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (!loggedUser.UserIsAdmin && loggedUser.UserId != userId)
        {
            _logger.LogWarning("User with ID {UserId} attempted to change password for user {TargetUserId} without permission.", currentUserId, userId);

            return new ApiResult<UserDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, userToUpdate.UserPasswordHash))
        {
            _logger.LogWarning("Incorrect current password provided for user {UserId}.", userId);

            return new ApiResult<UserDto>(
                InternalResultCode.INVALID_USER_PASSWORD,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (!dto.IsValidPassword())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.INVALID_USER_PASSWORD,
                HttpStatusCode.BadRequest,
                null);
        }

        if (!dto.PasswordsMatch())
        {
            return new ApiResult<UserDto>(
                InternalResultCode.PASSWORDS_DO_NOT_MATCH,
                HttpStatusCode.BadRequest,
                null);
        }

        string newHashedPassword = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);

        User updatedUser = await _userRepository.UpdatePasswordAsync(userToUpdate, newHashedPassword, loggedUser.UserName);

        await _unitOfWork.CommitAsync();

        return new ApiResult<UserDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            updatedUser.Adapt<UserDto>()
        );

    }

    /// <summary>
    /// Soft-deletes the user account and revokes all active sessions.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the user is not found.</exception>
    public async Task<ApiResult<EmptyDto>> DeleteAccountAsync(Guid userId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUserId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        User? userToDelete = await _userRepository.GetByIdAsync(userId);

        if (userToDelete == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", userId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        if (!loggedUser.UserIsAdmin && loggedUser.UserId != userId)
        {
            _logger.LogWarning("User with ID {UserId} attempted to delete user {TargetUserId} without permission.", currentUserId, userId);

            return new ApiResult<EmptyDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null
            );
        }

        _logger.LogInformation("Deleting user {UserId}.", userId);

        await _userRepository.SoftDeleteAsync(userToDelete, loggedUser.UserName);

        await _unitOfWork.CommitAsync();

        return new ApiResult<EmptyDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            null
        );
    }

}
