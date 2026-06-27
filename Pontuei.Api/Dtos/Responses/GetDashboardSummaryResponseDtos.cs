using System.Text.Json.Serialization;
using Pontuei.Api.Dtos.Objects;

namespace Pontuei.Api.Dtos.Responses;

public class GetDashboardSummaryResponseDto
{
    /// <summary>
    /// The top 3 loyalty programs with the most pending transactions for the user,
    /// </summary>
    [JsonPropertyName("topPrograms")]
    public List<DashboardProgramCardDto> TopPrograms { get; set; } = new();

    /// <summary>
    /// The "In Others" card, aggregating all programs from the 5th position onwards,
    /// or transactions from programs that the user has not favorited.
    /// </summary>
    [JsonPropertyName("others")]
    public DashboardOthersCardDto Others { get; set; } = new();
}

public class DashboardProgramCardDto
{
    [JsonPropertyName("loyaltyProgram")]
    public required LoyaltyProgramDto LoyaltyProgram { get; set; }

    [JsonPropertyName("totalPendingPoints")]
    public decimal TotalPendingPoints { get; set; }

    [JsonPropertyName("pendingTransactions")]
    public List<TransactionDto> PendingTransactions { get; set; } = new();
}

public class DashboardOthersCardDto
{
    [JsonPropertyName("totalPendingPoints")]
    public decimal TotalPendingPoints { get; set; }

    [JsonPropertyName("pendingTransactions")]
    public List<TransactionDto> PendingTransactions { get; set; } = new();
}