using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Response representing a single enrollment entry as shown in the carousel.
/// Includes the program branding details required to render the colored card.
/// </summary>
public class UserLoyaltyProgramResponseDto
{
    /// <summary>
    /// Enrollment record identifier.
    /// </summary>
    [JsonPropertyName("userLoyaltyProgramId")]
    public int UserLoyaltyProgramId { get; set; }

    /// <summary>
    /// Program summary with branding details for card rendering.
    /// </summary>
    [JsonPropertyName("loyaltyProgram")]
    public LoyaltyProgramDto LoyaltyProgram { get; set; } = null!;

    /// <summary>
    /// Position of this program's card in the user's home-screen carousel.
    /// </summary>
    [JsonPropertyName("displayOrder")]
    public short DisplayOrder { get; set; }
}
