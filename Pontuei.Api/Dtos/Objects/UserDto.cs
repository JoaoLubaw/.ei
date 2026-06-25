using System.Text.Json.Serialization;

namespace Pontuei.Api.Dtos.Objects;

/// <summary>
/// Lightweight projection used in list responses and nested references.
/// </summary>
public class UserBaseDto
{
    /// <summary>
    /// User's unique identifier.
    /// </summary>
    [JsonPropertyName("userId")]
    public Guid UserId { get; set; }

    /// <summary>
    /// User's display name.
    /// </summary>
    [JsonPropertyName("userName")]
    public string UserName { get; set; } = null!;

    /// <summary>
    /// User's e-mail address.
    /// </summary>
    [JsonPropertyName("userEmail")]
    public string UserEmail { get; set; } = null!;
}