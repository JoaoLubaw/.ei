using System.Text.Json.Serialization;
using Pontuei.Api.Enums;

namespace Pontuei.Api.Dtos.Requests;

/// <summary>
/// Payload for creating a new application configuration entry (admin operation).
/// Configuration rows store environment-level or feature-flag values consumed
/// by the API at runtime (e.g., default receipt deadline, points expiry rules).
/// </summary>
public class CreateConfigurationRequestDto
{
    /// <summary>
    /// Unique, human-readable key for this configuration entry (e.g., "DEFAULT_DEADLINE_DAYS").
    /// Required. Maps to <c>configuration_name</c>.
    /// </summary>
    [JsonPropertyName("configurationName")]
    public required string ConfigurationName { get; set; }

    /// <summary>
    /// Optional free-text explanation of what this configuration controls.
    /// Maps to <c>configuration_description</c>.
    /// </summary>
    [JsonPropertyName("configurationDescription")]
    public string? ConfigurationDescription { get; set; }

    /// <summary>
    /// The serialised value for this configuration entry.
    /// Interpretation depends on <see cref="ConfigurationType"/> (e.g., "30" for an integer,
    /// "true" for a boolean).
    /// Required. Maps to <c>configuration_value</c>.
    /// </summary>
    [JsonPropertyName("configurationValue")]
    public required string ConfigurationValue { get; set; }

    /// <summary>
    /// The data type of <see cref="ConfigurationValue"/>, used by consumers to
    /// deserialise the value correctly.
    /// Required. Maps to <c>configuration_type</c>.
    /// </summary>
    [JsonPropertyName("configurationType")]
    public required ConfigurationType ConfigurationType { get; set; }

    /// <summary>
    /// Returns <c>true</c> when all required fields are non-empty.
    /// </summary>
    public bool IsValid() =>
        !string.IsNullOrWhiteSpace(ConfigurationName) &&
        !string.IsNullOrWhiteSpace(ConfigurationValue);
}

/// <summary>
/// Payload for updating an existing configuration entry (admin operation).
/// All fields are optional — only non-null values are applied.
/// </summary>
public class UpdateConfigurationRequestDto
{
    /// <summary>
    /// Updated description. Optional.
    /// </summary>
    [JsonPropertyName("configurationDescription")]
    public string? ConfigurationDescription { get; set; }

    /// <summary>
    /// Updated serialised value. Optional.
    /// </summary>
    [JsonPropertyName("configurationValue")]
    public string? ConfigurationValue { get; set; }

    /// <summary>
    /// Updated type discriminator. Optional.
    /// </summary>
    [JsonPropertyName("configurationType")]
    public ConfigurationType? ConfigurationType { get; set; }
}
