using System.Net;
using Mapster;
using Microsoft.EntityFrameworkCore;

using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Interfaces;

using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;

using Pontuei.Api.Models;

namespace Pontuei.Api.Services;

/// <summary>
/// Business-logic contract for managing application configuration entries.
/// Provides typed value resolution helpers consumed by other services,
/// and full CRUD for admin-facing management endpoints.
/// </summary>
public class ConfigurationService : IConfigurationService
{
    private readonly IConfigurationRepository _configurationRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<ConfigurationService> _logger;

    public ConfigurationService(
        IConfigurationRepository configurationRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<ConfigurationService> logger
    )
    {
        _configurationRepository = configurationRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    /// <summary>
    /// Returns the full detail of a configuration entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the entry is not found.</exception>
    public async Task<ApiResult<ConfigurationDto>> GetByIdAsync(int configurationId, Guid CurrentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(CurrentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", CurrentUserId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (!loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User with ID {UserId} is not an admin.", CurrentUserId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null
            );
        }

        Configuration? configuration = await _configurationRepository.GetByIdAsync(configurationId);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration with ID {ConfigurationId} not found.", configurationId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        return new ApiResult<ConfigurationDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            configuration.Adapt<ConfigurationDto>()
        );
    }

    /// <summary>
    /// Returns all configuration entries for the admin management screen.
    /// </summary>
    public async Task<ApiResult<GetConfigurationsResponseDto>> GetAllAsync(GetConfigurationsRequestDto dto, Guid CurrentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(CurrentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", CurrentUserId);

            return new ApiResult<GetConfigurationsResponseDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (!loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User with ID {UserId} is not an admin.", CurrentUserId);

            return new ApiResult<GetConfigurationsResponseDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null
            );
        }

        IQueryable<Configuration> query = _configurationRepository.GetAll();
        query = ApplyFilters(query, dto.Filters);

        int totalElements = await query.CountAsync();
        int totalPages = (int)Math.Ceiling((double)totalElements / dto.Size);
        int skip = (dto.Page - 1) * dto.Size;

        List<Configuration> configurations = await query
            .Skip(skip)
            .Take(dto.Size)
            .ToListAsync();

        List<ConfigurationDto> configurationsDtos = configurations.Adapt<List<ConfigurationDto>>();

        _logger.LogInformation("Retrieved {Count} configurations for page {Page} with size {Size}. Total elements: {TotalElements}, Total pages: {TotalPages} For user {UserId}.",
            configurationsDtos.Count, dto.Page, dto.Size, totalElements, totalPages, CurrentUserId);

        return new ApiResult<GetConfigurationsResponseDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            new GetConfigurationsResponseDto
            {
                Configurations = configurationsDtos,
                Page = dto.Page,
                Size = dto.Size,
                TotalElements = totalElements,
                TotalPages = totalPages
            }
        );
    }

    /// <summary>
    /// Applies filters to the configuration query based on the provided filter DTO.
    /// </summary>
    /// <param name="query"></param>
    /// <param name="filters"></param>
    /// <returns></returns>
    private IQueryable<Configuration> ApplyFilters(IQueryable<Configuration> query, ConfigurationFiltersDto? filters)
    {
        if (filters == null)
        {
            return query;
        }

        if (filters.ConfigurationId.HasValue)
        {
            query = query.Where(c => c.ConfigurationId == filters.ConfigurationId.Value);
        }

        if (!string.IsNullOrWhiteSpace(filters.ConfigurationName))
        {
            query = query.Where(c => c.ConfigurationName.ToLower().Contains(filters.ConfigurationName.ToLower()));
        }

        if (filters.ConfigurationType.HasValue)
        {
            query = query.Where(c => c.ConfigurationType == filters.ConfigurationType.Value);
        }

        return query;
    }

    /// <summary>
    /// Applies partial updates to an existing configuration entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the entry is not found.</exception>
    public async Task<ApiResult<ConfigurationDto>> UpdateAsync(int configurationId, UpdateConfigurationRequestDto dto, Guid CurrentUserId)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(CurrentUserId);

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", CurrentUserId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        if (!loggedUser.UserIsAdmin)
        {
            _logger.LogWarning("User with ID {UserId} is not an admin.", CurrentUserId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.NOT_ADMIN,
                HttpStatusCode.Forbidden,
                null
            );
        }

        Configuration? configuration = await _configurationRepository.GetByIdAsync(configurationId);

        if (configuration == null)
        {
            _logger.LogWarning("Configuration with ID {ConfigurationId} not found.", configurationId);

            return new ApiResult<ConfigurationDto>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        _logger.LogInformation("Updating configuration with ID {ConfigurationId} for user {UserId}.", configurationId, CurrentUserId);

        Configuration updatedConfiguration = await _configurationRepository.UpdateAsync(configuration, dto, loggedUser.UserName);

        await _unitOfWork.CommitAsync();

        return new ApiResult<ConfigurationDto>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            updatedConfiguration.Adapt<ConfigurationDto>()
        );
    }
}
