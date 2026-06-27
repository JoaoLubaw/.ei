namespace Pontuei.Api.Interfaces.Services;

public interface IEmailService
{
    Task SendVerificationEmailAsync(string toEmail, string userName, string verificationCode);
    Task SendResetPasswordToken(string toEmail, string userName, string code);
    Task SendEmailChangeNotificationAsync(string toEmail, string completeName, string newEmail);
    Task SendEmailChangeCodeAsync(string toEmail, string completeName, string code);
}