using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Lightweight projection used in list responses and nested references.
/// </summary>
public class UserDto
{
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    [JsonPropertyName("userGoogleId")]
    public string? UserGoogleId { get; set; }

    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

    [JsonPropertyName("userEmail")]
    public required string UserEmail { get; set; }

    [JsonPropertyName("userPhoneNumber")]
    public string? UserPhoneNumber { get; set; }

    [JsonPropertyName("userEmailVerified")]
    public bool UserEmailVerified { get; set; }

    [JsonPropertyName("userEmailVerifiedAt")]
    public DateTime? UserEmailVerifiedAt { get; set; }

    [JsonPropertyName("userPushNotificationsEnabled")]
    public bool UserPushNotificationsEnabled { get; set; }

    [JsonPropertyName("userEmailNotificationsEnabled")]
    public bool UserEmailNotificationsEnabled { get; set; }

    [JsonPropertyName("userIsAdmin")]
    public bool UserIsAdmin { get; set; }
}