using Pontuei.Api.Dtos.Requests;
using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>configuration</c> table.
/// Configuration rows store application-level key-value settings consumed
/// by services at runtime (e.g., default deadline days, token lifetimes).
/// </summary>
public interface IConfigurationRepository
{
    /// <summary>
    /// Returns the configuration entry with the given <paramref name="configurationId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    Task<Configuration?> GetByIdAsync(int configurationId);

    /// <summary>
    /// Returns all configuration entries for the admin management screen.
    /// </summary>
    /// <returns></returns>
    IQueryable<Configuration> GetAll();

    /// <summary>
    /// Returns the configuration entry whose <c>configuration_name</c> matches
    /// <paramref name="name"/> (case-insensitive), or <c>null</c> when not found.
    /// </summary>
    Task<Configuration?> GetByNameAsync(string name);

    /// <summary>
    /// Applies changes to an existing configuration row and returns the updated entity.
    /// </summary>
    Task<Configuration> UpdateAsync(Configuration configuration, UpdateConfigurationRequestDto dto, string updatedBy);
}