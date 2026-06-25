using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Compact representation of a loyalty program used inside transaction and
/// notification responses, and in the card carousel on the home screen.
/// </summary>
public class LoyaltyProgramDto
{
    /// <summary>
    /// Loyalty program's unique identifier.
    /// </summary>
    [JsonPropertyName("loyaltyProgramId")]
    public int LoyaltyProgramId { get; set; }

    /// <summary>
    /// Human-readable name of the program (e.g., "Livelo", "Smiles").
    /// </summary>
    [JsonPropertyName("loyaltyProgramName")]
    public string LoyaltyProgramName { get; set; } = null!;

    /// <summary>
    /// URL of the program's logo image, used to render the cards in the carousel.
    /// </summary>
    [JsonPropertyName("loyaltyProgramLogoUrl")]
    public string? LoyaltyProgramLogoUrl { get; set; }

    /// <summary>
    /// Brand primary color (hex) used to tint the card background.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandPrimaryColor")]
    public string? LoyaltyProgramBrandPrimaryColor { get; set; }

    /// <summary>
    /// Brand secondary color (hex) for accents on the card.
    /// </summary>
    [JsonPropertyName("loyaltyProgramBrandSecondaryColor")]
    public string? LoyaltyProgramBrandSecondaryColor { get; set; }
}