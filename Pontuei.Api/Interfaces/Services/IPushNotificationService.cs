namespace Pontuei.Api.Interfaces.Services;

public interface IPushNotificationService
{
    Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken = default);
}