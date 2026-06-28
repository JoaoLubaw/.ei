using Pontuei.Api.Models;
using Pontuei.Shared.Dtos;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Interfaces.Repositories;
using System.Net;
using System.Reflection;

namespace Pontuei.Api.Services;

public class MetadataService : IMetadataService
{
    private readonly IMetadataRepository _metadataRepository;
    private readonly IUserRepository _userRepository;
    private readonly ILogger<MetadataService> _logger;

    public MetadataService(
        IMetadataRepository metadataRepository,
        IUserRepository userRepository,
        ILogger<MetadataService> logger)
    {
        _metadataRepository = metadataRepository;
        _userRepository = userRepository;
        _logger = logger;
    }

    /// <summary>
    /// Returns the current database version, or <c>null</c> when no version row exists.
    /// </summary>
    /// <returns></returns>
    public async Task<ApiResult<VersionsDtos?>> GetCurrentVersionsAsync(string currentUser)
    {
        User? loggedUser = await _userRepository.GetByIdAsync(Guid.Parse(currentUser));

        if (loggedUser == null)
        {
            _logger.LogWarning("User with ID {UserId} not found.", currentUser);

            return new ApiResult<VersionsDtos?>(
                InternalResultCode.UNLOGGED,
                HttpStatusCode.Unauthorized,
                null
            );
        }

        DbVersion? dbVersion = await _metadataRepository.GetCurrentDbVersionAsync();

        if (dbVersion == null)
        {
            _logger.LogWarning("No database version found to show user {UserId}.", currentUser);

            return new ApiResult<VersionsDtos?>(
                InternalResultCode.ENTITY_NOT_FOUND,
                HttpStatusCode.NotFound,
                null
            );
        }

        Assembly assembly = Assembly.GetExecutingAssembly();
        Version? version = assembly.GetName().Version;

        VersionsDtos versionsDtos = new VersionsDtos
        {
            VersionNumber = dbVersion.VersionNumber,
            DbVersionNotes = dbVersion.VersionNotes,
            ApiVersion = version?.ToString() ?? "Unknown"
        };

        _logger.LogInformation("Retrieved database version {DbVersionId} for user {UserId}.", dbVersion.DbVersionId, currentUser);

        return new ApiResult<VersionsDtos?>(
            InternalResultCode.NO_ERROR,
            HttpStatusCode.OK,
            versionsDtos
        );
    }
}