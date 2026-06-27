using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Requests;


/// <summary>
/// Payload for retrieving a paginated list of notifications for the authenticated user.
/// </summary>
public class GetNotificationsRequestDto
{
    public int Page { get; set; } = 1;
    public int Size { get; set; } = 10;
    public NotificationFiltersDto? Filters { get; set; }
}

/// <summary>
/// Filters for retrieving a paginated list of notifications for the authenticated user.
/// </summary>
public class NotificationFiltersDto
{
    public Guid? NotificationId { get; set; }
    public Guid? TransactionId { get; set; }
    public int? LoyaltyProgramId { get; set; }
    public bool? NotificationIsRead { get; set; }
}