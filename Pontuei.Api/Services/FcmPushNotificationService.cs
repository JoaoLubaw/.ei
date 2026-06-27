using FirebaseAdmin.Messaging;
using Pontuei.Api.Interfaces.Services;

namespace Pontuei.Api.Services;

/// <summary>
/// Sends push notifications to devices using Firebase Cloud Messaging (FCM).
/// </summary>
public class FcmPushNotificationService : IPushNotificationService
{
    private readonly ILogger<FcmPushNotificationService> _logger;

    public FcmPushNotificationService(ILogger<FcmPushNotificationService> logger)
    {
        _logger = logger;
    }

    public async Task SendAsync(string deviceToken, string title, string body, CancellationToken cancellationToken = default)
    {
        Message message = new Message
        {
            Token = deviceToken,
            Notification = new Notification
            {
                Title = title,
                Body = body
            },
            Android = new AndroidConfig
            {
                Priority = Priority.High
            }
        };

        try
        {
            string messageId = await FirebaseMessaging.DefaultInstance.SendAsync(message, cancellationToken);

            _logger.LogInformation("Push notification sent. MessageId: {MessageId}", messageId);
        }
        catch (FirebaseMessagingException ex) when (ex.MessagingErrorCode == MessagingErrorCode.Unregistered)
        {
            _logger.LogWarning("FCM token is no longer valid for device. Token prefix: {Prefix}...",
                deviceToken[..Math.Min(10, deviceToken.Length)]);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send push notification.");
            throw;
        }
    }
}