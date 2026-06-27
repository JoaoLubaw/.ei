using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Objects;
using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Dtos.Responses;
using Pontuei.Api.Enums;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// Business-logic contract for managing application configuration entries.
/// Provides typed value resolution helpers consumed by other services,
/// and full CRUD for admin-facing management endpoints.
/// </summary>
public interface IConfigurationService
{
    /// <summary>
    /// Returns the full detail of a configuration entry.
    /// </summary>
    Task<ApiResult<ConfigurationDto>> GetByIdAsync(int configurationId, Guid CurrentUserId);

    /// <summary>
    /// Returns the JWT expiration time in minutes, as configured in the <c>configuration</c> table.
    /// </summary>
    /// <returns></returns>
    Task<int> GetJWTExpirationMinutes();

    /// <summary>
    /// Returns all configuration entries for the admin management screen.
    /// </summary>
    Task<ApiResult<GetConfigurationsResponseDto>> GetAllAsync(GetConfigurationsRequestDto dto, Guid CurrentUserId);

    /// <summary>
    /// Applies partial updates to an existing configuration entry.
    /// </summary>
    Task<ApiResult<ConfigurationDto>> UpdateAsync(int configurationId, UpdateConfigurationRequestDto dto, Guid CurrentUserId);
}
