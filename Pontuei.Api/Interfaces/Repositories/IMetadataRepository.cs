using Pontuei.Api.Models;

namespace Pontuei.Api.Interfaces.Repositories;

/// <summary>
/// Data-access contract for the <c>db_version</c> table.
/// </summary>
public interface IMetadataRepository
{
    /// <summary>
    /// Returns the current database version, or <c>null</c> when no version row exists.
    /// </summary>
    /// <returns></returns>
    Task<DbVersion?> GetCurrentDbVersionAsync();
}