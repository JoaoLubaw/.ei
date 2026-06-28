using Pontuei.Api.Models;
using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Objects;

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
    Task<ApiResult<VersionsDtos?>> GetCurrentVersionsAsync(string currentUser);
}