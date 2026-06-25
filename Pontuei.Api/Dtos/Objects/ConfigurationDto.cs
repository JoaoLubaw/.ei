using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Full configuration entry returned to admin consumers.
/// </summary>
public class ConfigurationDto
{
    /// <summary>
    /// Configuration entry identifier.
    /// </summary>
    [JsonPropertyName("configurationId")]
    public int ConfigurationId { get; set; }

    /// <summary>
    /// Unique key name.
    /// </summary>
    [JsonPropertyName("configurationName")]
    public string ConfigurationName { get; set; } = null!;

    /// <summary>
    /// Human-readable description of what this entry controls.
    /// </summary>
    [JsonPropertyName("configurationDescription")]
    public string? ConfigurationDescription { get; set; }

    /// <summary>
    /// Serialised value string.
    /// </summary>
    [JsonPropertyName("configurationValue")]
    public string ConfigurationValue { get; set; } = null!;

    /// <summary>
    /// Type discriminator used for deserialisation.
    /// </summary>
    [JsonPropertyName("configurationType")]
    public ConfigurationType ConfigurationType { get; set; }
}
