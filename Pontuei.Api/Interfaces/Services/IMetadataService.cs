using Pontuei.Api.Models;
using Pontuei.Api.Dtos;
using Pontuei.Api.Dtos.Objects;

namespace Pontuei.Api.Interfaces.Services;

/// <summary>
/// 
/// </summary>
public interface IMetadataService
{
    /// <summary>
    /// Returns the current database version, or <c>null</c> when no version row exists.
    /// </summary>
    /// <returns></returns>
    Task<ApiResult<DbVersion?>> GetCurrentDbVersionAsync();
}