using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Objects;

/// <summary>
/// Response representing a single enrollment entry as shown in the carousel.
/// Includes the program branding details required to render the colored card.
/// </summary>
public class UserLoyaltyProgramDto
{
    [JsonPropertyName("userLoyaltyProgramId")]
    public int UserLoyaltyProgramId { get; set; }

    [JsonPropertyName("loyaltyProgram")]
    public required LoyaltyProgramDto LoyaltyProgram { get; set; }

    [JsonPropertyName("userLoyaltyProgramDisplayOrder")]
    public short UserLoyaltyProgramDisplayOrder { get; set; }
}