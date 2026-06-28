using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Objects;

/// <summary>
/// Compact representation of a loyalty program used inside transaction and
/// notification responses, and in the card carousel on the home screen.
/// </summary>
public class LoyaltyProgramDto
{
    [JsonPropertyName("loyaltyProgramId")]
    public int LoyaltyProgramId { get; set; }

    [JsonPropertyName("loyaltyProgramName")]
    public required string LoyaltyProgramName { get; set; }

    [JsonPropertyName("loyaltyProgramLogoUrl")]
    public string? LoyaltyProgramLogoUrl { get; set; }

    [JsonPropertyName("loyaltyProgramBrandPrimaryColor")]
    public string? LoyaltyProgramBrandPrimaryColor { get; set; }

    [JsonPropertyName("loyaltyProgramBrandSecondaryColor")]
    public string? LoyaltyProgramBrandSecondaryColor { get; set; }

    [JsonPropertyName("loyaltyProgramIsActive")]
    public bool LoyaltyProgramIsActive { get; set; }
}