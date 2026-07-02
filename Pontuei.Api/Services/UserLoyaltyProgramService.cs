using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Shared.Enums;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

public class UserLoyaltyProgramService : IUserLoyaltyProgramService
{
    private readonly IUserLoyaltyProgramRepository _userLoyaltyProgramRepository;
    private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<UserLoyaltyProgramService> _logger;

    public UserLoyaltyProgramService(
        IUserLoyaltyProgramRepository userLoyaltyProgramRepository,
        ILoyaltyProgramRepository loyaltyProgramRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<UserLoyaltyProgramService> logger)
    {
        _userLoyaltyProgramRepository = userLoyaltyProgramRepository;
        _loyaltyProgramRepository = loyaltyProgramRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<ApiResult<GetUserLoyaltyProgramsResponseDto>> GetByUserIdAsync(Guid userId, GetUserLoyaltyProgramsRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to access loyalty programs for user {UserId} without permission.", currentUserId, userId);
            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_GET_THIS_USER,
                HttpStatusCode.Forbidden,
                null);
        }

        IQueryable<UserLoyaltyProgram> query = _userLoyaltyProgramRepository.GetByUserIdAsync(userId);

        if (dto.Filters != null)
        {
            if (dto.Filters.UserLoyaltyProgramId.HasValue)
                query = query.Where(ulp => ulp.UserLoyaltyProgramId == dto.Filters.UserLoyaltyProgramId.Value);

            if (dto.Filters.LoyaltyProgramId.HasValue)
                query = query.Where(ulp => ulp.LoyaltyProgramId == dto.Filters.LoyaltyProgramId.Value);
        }

        List<UserLoyaltyProgram> dbUserPrograms = await query.ToListAsync();
        HashSet<int> enrolledProgramIds = dbUserPrograms.Select(up => up.LoyaltyProgramId).ToHashSet();

        List<UserLoyaltyProgramDto> mergedList = dbUserPrograms.Adapt<List<UserLoyaltyProgramDto>>();

        if (dto.Filters?.UserLoyaltyProgramId == null && dto.Filters?.LoyaltyProgramId == null)
        {
            List<LoyaltyProgram> allActivePrograms = await _loyaltyProgramRepository.GetAllAsync()
                .Where(lp => lp.LoyaltyProgramIsActive)
                .ToListAsync();

            foreach (LoyaltyProgram program in allActivePrograms)
            {
                if (enrolledProgramIds.Contains(program.LoyaltyProgramId)) continue;

                mergedList.Add(new UserLoyaltyProgramDto
                {
                    UserLoyaltyProgramId = 0,
                    UserLoyaltyProgramDisplayOrder = 4,
                    LoyaltyProgram = program.Adapt<LoyaltyProgramDto>()
                });
            }
        }

        mergedList = mergedList
            .OrderBy(p => p.UserLoyaltyProgramDisplayOrder)
            .ThenBy(p => p.LoyaltyProgram?.LoyaltyProgramName)
            .ToList();

        int totalElements = mergedList.Count;
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<UserLoyaltyProgramDto> pagedList = mergedList.Skip(skip).Take(dto.Size).ToList();

        return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetUserLoyaltyProgramsResponseDto
            {
                Page = dto.Page,
                Size = dto.Size,
                TotalElements = totalElements,
                TotalPages = totalPages,
                UserLoyaltyPrograms = pagedList
            }
        );
    }

    public async Task<ApiResult<UserLoyaltyProgramDto>> EnrollAsync(Guid userId, CreateUserLoyaltyProgramRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<UserLoyaltyProgramDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to enroll user {UserId} in a program without permission.", currentUserId, userId);

            return new ApiResult<UserLoyaltyProgramDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null);
        }

        LoyaltyProgram? program = await _loyaltyProgramRepository.GetByIdAsync(dto.LoyaltyProgramId, VerifyActive: true);

        if (program == null)
        {
            _logger.LogWarning("Attempt to enroll in inactive or non-existent loyalty program: {ProgramId}", dto.LoyaltyProgramId);

            return new ApiResult<UserLoyaltyProgramDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        UserLoyaltyProgram? existingEnrollment = await _userLoyaltyProgramRepository.GetAsync(userId, dto.LoyaltyProgramId);

        if (existingEnrollment != null)
        {
            _logger.LogInformation("User {UserId} is already enrolled in loyalty program {ProgramId}.", userId, dto.LoyaltyProgramId);

            return new ApiResult<UserLoyaltyProgramDto>(
                InternalResultCode.INFO_ALREADY_EXISTS,
                HttpStatusCode.Conflict,
                null);
        }

        UserLoyaltyProgram newEnrollment = await _userLoyaltyProgramRepository.CreateAsync(dto, userId, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to save loyalty program enrollment for user: {UserId}", userId);

            return new ApiResult<UserLoyaltyProgramDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null);
        }

        newEnrollment.LoyaltyProgram = program;

        _logger.LogInformation("Successfully enrolled user {UserId} into program {ProgramId} by user {CurrentUserId}.", userId, dto.LoyaltyProgramId, currentUserId);

        return new ApiResult<UserLoyaltyProgramDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.Created,
            newEnrollment.Adapt<UserLoyaltyProgramDto>()
        );
    }

    public async Task<ApiResult<GetUserLoyaltyProgramsResponseDto>> BulkUpdateAsync(Guid userId, BulkUpdateUserLoyaltyProgramsRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to bulk update programs for user {UserId} without permission.", currentUserId, userId);

            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                null);
        }

        User? targetUser = userId == currentUserId ? loggedUser : await _userRepository.GetByIdAsync(userId);

        if (targetUser == null)
        {
            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        List<int> requestedIds = dto.Programs.Select(p => p.LoyaltyProgramId).Distinct().ToList();
        foreach (int programId in requestedIds)
        {
            LoyaltyProgram? program = await _loyaltyProgramRepository.GetByIdAsync(programId, VerifyActive: true);
            if (program == null)
            {
                _logger.LogWarning("Bulk update failed: Program {ProgramId} is inactive or does not exist.", programId);
                return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                    InternalResultCode.ENTITY_NOT_FOUND,
                    HttpStatusCode.NotFound,
                    null);
            }
        }

        await _userLoyaltyProgramRepository.BulkUpdateAsync(targetUser, dto, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to save bulk update for user: {UserId}", userId);

            return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null);
        }

        List<UserLoyaltyProgram> updatedList = await _userLoyaltyProgramRepository.GetByUserIdAsync(userId).ToListAsync();

        _logger.LogInformation("Successfully bulk updated loyalty programs for user {UserId}.", userId);

        return new ApiResult<GetUserLoyaltyProgramsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetUserLoyaltyProgramsResponseDto
            {
                Page = 1,
                Size = updatedList.Count == 0 ? 10 : updatedList.Count,
                TotalElements = updatedList.Count,
                TotalPages = 1,
                UserLoyaltyPrograms = updatedList.Adapt<List<UserLoyaltyProgramDto>>()
            }
        );
    }

    public async Task<ApiResult<bool>> UnenrollAsync(Guid userId, int loyaltyProgramId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<bool>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                false);
        }

        if (userId != currentUserId && !loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User {CurrentUserId} attempted to unenroll user {UserId} without permission.", currentUserId, userId);

            return new ApiResult<bool>(
                InternalResultCode.NOT_ALLOWED_TO_EDIT_USER,
                HttpStatusCode.Forbidden,
                false);
        }

        UserLoyaltyProgram? enrollment = await _userLoyaltyProgramRepository.GetAsync(userId, loyaltyProgramId);

        if (enrollment == null)
        {
            return new ApiResult<bool>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                false);
        }

        await _userLoyaltyProgramRepository.DeleteAsync(enrollment, currentUserId.ToString());

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to remove loyalty program enrollment {ProgramId} for user: {UserId}", loyaltyProgramId, userId);

            return new ApiResult<bool>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                false);
        }

        _logger.LogInformation("Successfully unenrolled user {UserId} from program {ProgramId}.", userId, loyaltyProgramId);

        return new ApiResult<bool>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            true
        );
    }
}