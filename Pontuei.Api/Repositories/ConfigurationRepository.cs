using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Data;

using Pontuei.Shared.Dtos.Requests;
using Pontuei.Api.Interfaces.Repositories;

using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class ConfigurationRepository : BaseRepository, IConfigurationRepository
{
    public ConfigurationRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the configuration entry with the given <paramref name="configurationId"/>,
    /// or <c>null</c> when not found.
    /// </summary>
    public async Task<Configuration?> GetByIdAsync(int configurationId)
    {
        return await _dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigurationId == configurationId);
    }

    /// <summary>
    /// Returns all configuration entries for the admin management screen.
    /// </summary>
    /// <returns></returns>
    public IQueryable<Configuration> GetAll()
    {
        return _dbContext.Configurations;
    }

    /// <summary>
    /// Returns the configuration entry whose <c>configuration_name</c> matches
    /// <paramref name="name"/> (case-insensitive), or <c>null</c> when not found.
    /// </summary>
    public async Task<Configuration?> GetByNameAsync(string name)
    {
        return await _dbContext.Configurations.FirstOrDefaultAsync(c => c.ConfigurationName.ToLower() == name.ToLower());
    }


    /// <summary>
    /// Applies changes to an existing configuration row and returns the updated entity.
    /// </summary>
    public Task<Configuration> UpdateAsync(Configuration configuration, UpdateConfigurationRequestDto dto, string updatedBy)
    {
        _dbContext.Attach(configuration);

        if (dto.ConfigurationDescription != null && dto.ConfigurationDescription != configuration.ConfigurationDescription)
        {
            configuration.ConfigurationDescription = dto.ConfigurationDescription;
            _dbContext.Entry(configuration).Property(c => c.ConfigurationDescription).IsModified = true;
        }

        if (dto.ConfigurationValue != null && dto.ConfigurationValue != configuration.ConfigurationValue)
        {
            configuration.ConfigurationValue = dto.ConfigurationValue;
            _dbContext.Entry(configuration).Property(c => c.ConfigurationValue).IsModified = true;
        }

        configuration.UpdateTime = DateTime.UtcNow;
        _dbContext.Entry(configuration).Property(c => c.UpdateTime).IsModified = true;

        configuration.UpdateUser = updatedBy;
        _dbContext.Entry(configuration).Property(c => c.UpdateUser).IsModified = true;

        return Task.FromResult(configuration);
    }

}