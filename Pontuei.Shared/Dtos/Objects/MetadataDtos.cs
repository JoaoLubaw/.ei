using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Objects;

public class VersionsDtos
{
    /// <summary>
    /// The version number of the database.
    /// </summary>
    [JsonPropertyName("versionNumber")]
    public required string VersionNumber { get; set; }

    /// <summary>
    /// Notes about the database version.
    /// </summary>
    [JsonPropertyName("dbVersionNotes")]
    public string? DbVersionNotes { get; set; }

    /// <summary>
    /// The version number of the API.
    /// </summary>
    [JsonPropertyName("apiVersion")]
    public required string ApiVersion { get; set; }
}