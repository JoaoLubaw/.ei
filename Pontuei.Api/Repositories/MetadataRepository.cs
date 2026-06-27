using Microsoft.EntityFrameworkCore;
using Pontuei.Api.Data;

using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

namespace Pontuei.Api.Repositories;

public class MetadataRepository : BaseRepository, IMetadataRepository
{
    public MetadataRepository(PontueiDbContext dbContext) : base(dbContext)
    {
    }

    /// <summary>
    /// Returns the current database version, or <c>null</c> when no version row exists.
    /// </summary>
    /// <returns></returns>
    public async Task<DbVersion?> GetCurrentDbVersionAsync()
    {
        return await _dbContext.DbVersions.OrderByDescending(db => db.DbVersionId).FirstAsync();
    }
}