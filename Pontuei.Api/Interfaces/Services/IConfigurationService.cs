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
    // ── Admin CRUD ────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the full detail of a configuration entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the entry is not found.</exception>
    Task<ApiResult<ConfigurationDto>> GetByIdAsync(int configurationId, Guid CurrentUserId);

    /// <summary>
    /// Returns all configuration entries for the admin management screen.
    /// </summary>
    Task<ApiResult<GetConfigurationsResponseDto>> GetAllAsync(GetConfigurationsRequestDto dto, Guid CurrentUserId);

    /// <summary>
    /// Creates a new configuration entry.
    /// Validates key uniqueness before persisting.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when a configuration with the same name already exists.</exception>
    Task<ApiResult<ConfigurationDto>> CreateAsync(CreateConfigurationRequestDto dto, Guid CurrentUserId);

    /// <summary>
    /// Applies partial updates to an existing configuration entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the entry is not found.</exception>
    Task<ApiResult<ConfigurationDto>> UpdateAsync(int configurationId, UpdateConfigurationRequestDto dto, Guid CurrentUserId);

    /// <summary>
    /// Soft-deletes a configuration entry.
    /// </summary>
    /// <exception cref="KeyNotFoundException">Thrown when the entry is not found.</exception>
    Task<ApiResult<bool>> DeleteAsync(int configurationId, Guid CurrentUserId);
}
