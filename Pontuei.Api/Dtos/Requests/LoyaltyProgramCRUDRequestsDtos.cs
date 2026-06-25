using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos;

/// <summary>
/// Payload for creating a new loyalty program entry (admin operation).
/// Programs appear as selectable cards in the onboarding screen and in the
/// transaction "Programa de pontuação" picker.
/// </summary>
public class CreateLoyaltyProgramRequestDto
{
    /// <summary>
    /// Display name of the program (e.g., "Livelo", "Esfera", "Smiles").
    /// Required. Maps to <c>loyalty_program_name</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramName")]
    public required string LoyaltyProgramName { get; set; }

    /// <summary>
    /// Public URL of the program's logo image.
    /// Optional. Maps to <c>loyalty_program_logo_url</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramLogoUrl")]
    public string? LoyaltyProgramLogoUrl { get; set; }

    /// <summary>
    /// Primary brand color in hex format (e.g., <c>"#E91E8C"</c> for Livelo).
    /// Optional. Maps to <c>loyalty_program_brand_primary_color</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandPrimaryColor")]
    public string? LoyaltyProgramBrandPrimaryColor { get; set; }

    /// <summary>
    /// Secondary brand color in hex format.
    /// Optional. Maps to <c>loyalty_program_brand_secondary_color</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandSecondaryColor")]
    public string? LoyaltyProgramBrandSecondaryColor { get; set; }

    /// <summary>
    /// Whether the program is active and visible to users.
    /// Defaults to <c>true</c>. Maps to <c>loyalty_program_is_active</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramIsActive")]
    public bool LoyaltyProgramIsActive { get; set; } = true;

    /// <summary>
    /// Returns <c>true</c> when the required name field is present.
    /// </summary>
    public bool IsValid() => !string.IsNullOrWhiteSpace(LoyaltyProgramName);
}

/// <summary>
/// Payload for updating an existing loyalty program (admin operation).
/// All fields are optional — only non-null values are applied.
/// </summary>
public class UpdateLoyaltyProgramRequestDto
{
    /// <summary>
    /// Updated display name. Optional.
    /// </summary>
    [JsonPropertyName("loyaltyProgramName")]
    public string? LoyaltyProgramName { get; set; }

    /// <summary>
    /// Updated logo URL. Optional.
    /// </summary>
    [JsonPropertyName("loyaltyProgramLogoUrl")]
    public string? LoyaltyProgramLogoUrl { get; set; }

    /// <summary>
    /// Updated primary brand color in hex. Optional.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandPrimaryColor")]
    public string? LoyaltyProgramBrandPrimaryColor { get; set; }

    /// <summary>
    /// Updated secondary brand color in hex. Optional.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandSecondaryColor")]
    public string? LoyaltyProgramBrandSecondaryColor { get; set; }

    /// <summary>
    /// Toggling this to <c>false</c> hides the program from users without deleting it.
    /// Optional.
    /// </summary>
    [JsonPropertyName("loyaltyProgramIsActive")]
    public bool? LoyaltyProgramIsActive { get; set; }
}
