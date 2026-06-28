using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;
using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Shared.Dtos.Requests;
using Pontuei.Shared.Dtos.Responses;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

/// <summary>
/// Business-logic contract for managing the global loyalty program catalogue (admin operations).
/// </summary>
public class LoyaltyProgramService : ILoyaltyProgramService
{
    private readonly ILoyaltyProgramRepository _loyaltyProgramRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<LoyaltyProgramService> _logger;

    public LoyaltyProgramService(
        ILoyaltyProgramRepository loyaltyProgramRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<LoyaltyProgramService> logger)
    {
        _loyaltyProgramRepository = loyaltyProgramRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Returns the full loyalty program detail for a given ID.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the program does not exist.</exception>
    public async Task<ApiResult<LoyaltyProgramDto>> GetByIdAsync(int loyaltyProgramId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);
        if (loggedUser == null)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        LoyaltyProgram? program = await _loyaltyProgramRepository.GetByIdAsync(loyaltyProgramId, true);
        if (program == null)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        return new ApiResult<LoyaltyProgramDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            program.Adapt<LoyaltyProgramDto>()
        );
    }

    /// <summary>
    /// Returns all loyalty programs available for user enrollment,
    /// ordered by name. Only active programs are returned to non-admin callers.
    /// </summary>
    public async Task<ApiResult<GetLoyaltyProgramsResponseDto>> GetAllAsync(GetLoyaltyProgramsRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);
        if (loggedUser == null)
        {
            return new ApiResult<GetLoyaltyProgramsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        IQueryable<LoyaltyProgram> query = _loyaltyProgramRepository.GetAllAsync();
        query = ApplyFilters(query, dto);

        if (!loggedUser.UserIsAdmin)
        {
            query = query.Where(lp => lp.LoyaltyProgramIsActive);
        }
        else if (loggedUser.UserIsAdmin && dto.Filters?.LoyaltyProgramIsActive.HasValue == true)
        {
            query = query.Where(lp => lp.LoyaltyProgramIsActive == dto.Filters.LoyaltyProgramIsActive.Value);
        }

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<LoyaltyProgram> programs = await query
            .Skip(skip)
            .Take(dto.Size)
            .ToListAsync();

        return new ApiResult<GetLoyaltyProgramsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetLoyaltyProgramsResponseDto
            {
                Page = dto.Page,
                Size = dto.Size,
                TotalElements = totalElements,
                TotalPages = totalPages,
                LoyaltyPrograms = programs.Adapt<List<LoyaltyProgramDto>>()
            }
        );
    }

    private IQueryable<LoyaltyProgram> ApplyFilters(IQueryable<LoyaltyProgram> query, GetLoyaltyProgramsRequestDto requestDto)
    {
        if (requestDto.Filters == null)
        {
            return query.OrderBy(lp => lp.LoyaltyProgramName);
        }

        if (!string.IsNullOrWhiteSpace(requestDto.Filters.LoyaltyProgramName))
        {
            query = query.Where(p => p.LoyaltyProgramName != null &&
                 EF.Functions.ILike(p.LoyaltyProgramName, requestDto.Filters.LoyaltyProgramName + "%"));
        }

        if (requestDto.Filters.LoyaltyProgramIsActive.HasValue)
        {
            query = query.Where(lp => lp.LoyaltyProgramIsActive == requestDto.Filters.LoyaltyProgramIsActive.Value);
        }

        return query.OrderBy(lp => lp.LoyaltyProgramName);
    }

    /// <summary>
    /// Creates a new loyalty program in the catalogue.
    /// Validates name uniqueness before persisting.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a program with the same name already exists.</exception>
    /// <summary>
    /// Creates a new loyalty program in the catalogue.
    /// Validates name uniqueness before persisting.
    /// </summary>
    public async Task<ApiResult<LoyaltyProgramDto>> CreateAsync(CreateLoyaltyProgramRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (!loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("Attempted to create loyalty program by non-admin user: {UserId}", currentUserId);

            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null);
        }

        if (!dto.IsValid())
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.MISSING_INFORMATION,
                HttpStatusCode.BadRequest,
                null);
        }

        LoyaltyProgram? existingProgram = await _loyaltyProgramRepository.GetByNameAsync(dto.LoyaltyProgramName);

        if (existingProgram != null)
        {
            _logger.LogWarning("Attempted to create duplicate loyalty program: {ProgramName}", dto.LoyaltyProgramName);

            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.INFO_ALREADY_EXISTS,
                HttpStatusCode.Conflict,
                null);
        }

        LoyaltyProgram newProgram = await _loyaltyProgramRepository.CreateAsync(dto, loggedUser.UserName);

        bool saved = await _unitOfWork.CommitAsync();

        if (!saved)
        {
            _logger.LogError("Failed to save loyalty program: {ProgramName}", dto.LoyaltyProgramName);

            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.DATABASE_CONNECTION,
                HttpStatusCode.InternalServerError,
                null);
        }

        return new ApiResult<LoyaltyProgramDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.Created,
            newProgram.Adapt<LoyaltyProgramDto>()
        );
    }

    /// <summary>
    /// Applies partial updates to an existing loyalty program.
    /// </summary>
    public async Task<ApiResult<LoyaltyProgramDto>> UpdateAsync(int loyaltyProgramId, UpdateLoyaltyProgramRequestDto dto, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null);
        }

        if (!loggedUser.UserIsAdmin)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null);
        }

        LoyaltyProgram? programToUpdate = await _loyaltyProgramRepository.GetByIdAsync(loyaltyProgramId, false);

        if (programToUpdate == null)
        {
            return new ApiResult<LoyaltyProgramDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null);
        }

        if (!string.IsNullOrWhiteSpace(dto.LoyaltyProgramName) &&
            dto.LoyaltyProgramName.ToLower() != programToUpdate.LoyaltyProgramName.ToLower())
        {
            bool nameExists = await _loyaltyProgramRepository.GetByNameAsync(dto.LoyaltyProgramName) != null;

            if (nameExists)
            {
                return new ApiResult<LoyaltyProgramDto>(
                    InternalResultCode.INFO_ALREADY_EXISTS,
                    HttpStatusCode.Conflict,
                    null);
            }
        }

        LoyaltyProgram updatedProgram = await _loyaltyProgramRepository.UpdateAsync(programToUpdate, dto, loggedUser.UserName);

        _logger.LogInformation("Loyalty program updated: {ProgramId} by user: {UserId}", loyaltyProgramId, currentUserId);

        await _unitOfWork.CommitAsync();

        return new ApiResult<LoyaltyProgramDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            updatedProgram.Adapt<LoyaltyProgramDto>()
        );
    }

    /// <summary>
    /// Soft-deletes a loyalty program from the catalogue.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the program is not found.</exception>
    public async Task<ApiResult<bool>> ToggleActiveAsync(int loyaltyProgramId, Guid currentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(currentUserId);

        if (loggedUser == null)
        {
            return new ApiResult<bool>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                false);
        }

        if (!loggedUser.UserIsAdmin)
        {
            return new ApiResult<bool>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                false);
        }

        LoyaltyProgram? program = await _loyaltyProgramRepository.GetByIdAsync(loyaltyProgramId, true);
        if (program == null)
        {
            return new ApiResult<bool>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                false);
        }

        await _loyaltyProgramRepository.ToggleActiveAsync(program, loggedUser.UserEmail);

        _logger.LogInformation("Loyalty program active status toggled: {ProgramId} by user: {UserId}. New status: {IsActive}", loyaltyProgramId, currentUserId, program.LoyaltyProgramIsActive);

        await _unitOfWork.CommitAsync();

        return new ApiResult<bool>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            program.LoyaltyProgramIsActive
        );
    }
}
