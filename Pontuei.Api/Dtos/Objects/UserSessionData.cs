namespace Pontuei.Api.Dtos.Objects;

public class UserSessionData
{
    public required string AccessToken { get; set; }
    public required string RefreshToken { get; set; }
    public string? DeviceInfo { get; set; }
    public DateTime CreatedAt { get; set; }
}