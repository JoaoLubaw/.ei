using System.Text.Json.Serialization;

namespace Pontuei.Shared.Dtos.Requests;

/// <summary>
/// Payload for enrolling the authenticated user in one or more loyalty programs.
/// Matches the onboarding screen ("A gente precisa saber... Quais os seus principais
/// sistemas de pontuação atualmente?") and the home-screen card-reorder flow.
/// </summary>
public class CreateUserLoyaltyProgramRequestDto
{
    /// <summary>
    /// Identifier of the loyalty program the user wants to enroll in.
    /// Maps to <c>loyalty_program_id</c>.
    /// </summary>
    [JsonPropertyName("loyaltyProgramId")]
    public required int LoyaltyProgramId { get; set; }

    /// <summary>
    /// Display order of this program's card in the home-screen carousel.
    /// Lower values appear first. Maps to <c>user_loyalty_program_display_order</c>.
    /// </summary>
    [JsonPropertyName("displayOrder")]
    public short DisplayOrder { get; set; } = 0;
}

/// <summary>
/// Payload for bulk-saving the authenticated user's program list with
/// updated ordering. Sent when the user taps "Salvar" on the card-reorder screen.
/// Replaces the existing enrollment list for the user.
/// </summary>
public class BulkUpdateUserLoyaltyProgramsRequestDto
{
    /// <summary>
    /// Ordered list of program enrollments to persist.
    /// The index position within the list determines the final display order,
    /// supplementing the explicit <c>DisplayOrder</c> field.
    /// </summary>
    [JsonPropertyName("programs")]
    public required List<CreateUserLoyaltyProgramRequestDto> Programs { get; set; }
}

public class GetUserLoyaltyProgramsRequestDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public UserLoyaltyProgramFiltersDto? Filters { get; set; }
}

public class UserLoyaltyProgramFiltersDto
{
    public int? UserLoyaltyProgramId { get; set; }
    public Guid? UserId { get; set; }
    public int? LoyaltyProgramId { get; set; }
}