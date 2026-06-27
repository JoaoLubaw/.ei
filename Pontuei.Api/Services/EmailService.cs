using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Options;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models.Settings;

namespace Pontuei.Api.Services;

public class EmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger)
    {
        _settings = settings.Value;
        _logger = logger;
    }

    public async Task SendVerificationEmailAsync(string toEmail, string userName, string verificationCode)
    {
        string subject = "Só mais um ponto! Confirme seu e-mail no .Ei";

        string content = $@"
            <h2 style='color: #1a1a1a; margin-top: 0;'>Olá, {userName}!</h2>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Estamos quase pontos*! Falta só confirmar seu e-mail, ele é muito importante pra gente (assim como você)!
            </p>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Use o código de 6 dígitos abaixo no aplicativo para concluir a criação da sua conta:
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <span style='background-color: #f0f0f0; border: 1px solid #dcdcdc; color: #1a1a1a; font-size: 28px; font-weight: bold; letter-spacing: 5px; padding: 15px 30px; border-radius: 12px; display: inline-block;'>
                    {verificationCode}
                </span>
            </div>
            <p style='color: #7a7a7a; font-size: 14px;'>Se você não se cadastrou no .Ei, pode apenas ignorar este e-mail. (Mas se quiser... Queremos também viu?)</p>";

        await SendEmailAsync(toEmail, subject, content);
    }

    public async Task SendResetPasswordToken(string toEmail, string userName, string code)
    {
        string subject = "Recuperação de Senha - .Ei";

        string content = $@"
            <h2 style='color: #1a1a1a; margin-top: 0;'>Esqueceu a senha, {userName}?</h2>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Relaxa, a gente super te entende! Vamos dar um jeito nisso agora mesmo.
            </p>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Para redefinir sua senha, digite o código de verificação abaixo no aplicativo:
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <span style='background-color: #e8f5e9; border: 1px solid #4caf50; color: #2e7d32; font-size: 28px; font-weight: bold; letter-spacing: 5px; padding: 15px 30px; border-radius: 12px; display: inline-block;'>
                    {code}
                </span>
            </div>
            <p style='color: #7a7a7a; font-size: 14px;'>Este código é válido por 30 minutos. Se você não solicitou a troca, ignore este e-mail por segurança.</p>";

        await SendEmailAsync(toEmail, subject, content);
    }

    public async Task SendEmailChangeNotificationAsync(string toEmail, string completeName, string newEmail)
    {
        string subject = "Alerta de Segurança: Seu e-mail foi alterado";

        string content = $@"
            <h2 style='color: #1a1a1a; margin-top: 0;'>Olá, {completeName}.</h2>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Estamos passando para avisar que o e-mail de acesso da sua conta no <strong>.Ei</strong> foi alterado com sucesso para:
            </p>
            <p style='text-align: center; font-size: 18px; font-weight: bold; color: #1a1a1a; margin: 20px 0;'>
                {newEmail}
            </p>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                A partir de agora, use este novo e-mail para fazer login no app.
            </p>
            <div style='background-color: #fff3cd; border-left: 4px solid #ffecb5; padding: 15px; margin-top: 25px; border-radius: 4px;'>
                <p style='color: #856404; font-size: 14px; margin: 0;'><strong>Aviso:</strong> Se você não reconhece essa alteração, mude sua senha imediatamente.</p>
            </div>";

        await SendEmailAsync(toEmail, subject, content);
    }

    public async Task SendEmailChangeCodeAsync(string toEmail, string completeName, string code)
    {
        string subject = "Código para alteração de e-mail";

        string content = $@"
            <h2 style='color: #1a1a1a; margin-top: 0;'>Olá, {completeName}.</h2>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Recebemos um pedido para alterar o e-mail associado à sua conta no .Ei.
            </p>
            <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
                Para confirmar e validar este novo e-mail, insira o código abaixo no aplicativo:
            </p>
            <div style='text-align: center; margin: 30px 0;'>
                <span style='background-color: #f0f0f0; border: 1px solid #dcdcdc; color: #1a1a1a; font-size: 28px; font-weight: bold; letter-spacing: 5px; padding: 15px 30px; border-radius: 12px; display: inline-block;'>
                    {code}
                </span>
            </div>
            <p style='color: #7a7a7a; font-size: 14px;'>Se você não solicitou essa mudança em uma conta .Ei, ignore esta mensagem.</p>";

        await SendEmailAsync(toEmail, subject, content);
    }

    /// <summary>
    /// Envia o e-mail, encapsulando o conteúdo fornecido dentro do HTML base do Pontuei.
    /// </summary>
    private async Task SendEmailAsync(string toEmail, string subject, string innerHtmlContent)
    {
        try
        {
            string finalHtmlBody = BuildHtmlTemplate(subject, innerHtmlContent);

            using MailMessage mail = new MailMessage
            {
                From = new MailAddress(_settings.From, _settings.FromName),
                Subject = subject,
                Body = finalHtmlBody,
                IsBodyHtml = true
            };

            mail.To.Add(toEmail);

            using SmtpClient smtp = new SmtpClient(_settings.Host, _settings.Port)
            {
                Credentials = new NetworkCredential(_settings.Username, _settings.Password),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);

            _logger.LogInformation("Email '{Subject}' successfully sent to {ToEmail}", subject, toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email '{Subject}' to {ToEmail}", subject, toEmail);
            throw;
        }
    }

    public async Task SendOverdueTransactionEmailAsync(
        string toEmail, string userName, string store,
        string programName, DateOnly deadline, int estimatedPoints)
    {
        string subject = "Eii, um prazo de pontuação foi expirado - .Ei";

        string content = $@"
        <h2 style='color: #1a1a1a; margin-top: 0;'>Atenção, {userName}!</h2>
        <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
            O prazo para creditação dos seus pontos expirou e você ainda não identificou o crédito na sua conta.
        </p>
        <div style='background-color: #fff8e1; border-left: 4px solid #ffc107; padding: 15px; margin: 20px 0; border-radius: 4px;'>
            <p style='margin: 0 0 8px 0; color: #5d4037; font-size: 15px;'><strong>🏪 Loja:</strong> {store}</p>
            <p style='margin: 0 0 8px 0; color: #5d4037; font-size: 15px;'><strong>🎯 Programa:</strong> {programName}</p>
            <p style='margin: 0 0 8px 0; color: #5d4037; font-size: 15px;'><strong>📅 Prazo era:</strong> {deadline:dd/MM/yyyy}</p>
            <p style='margin: 0; color: #5d4037; font-size: 15px;'><strong>✈️ Pontos esperados:</strong> {estimatedPoints:N0} pts</p>
        </div>
        <p style='color: #4a4a4a; font-size: 16px; line-height: 1.5;'>
            Acesse o aplicativo e verifique se os pontos foram creditados. 
            Se não, pode ser um bom momento para entrar em contato com o(a) {programName}.
        </p>
        <p style='color: #7a7a7a; font-size: 14px;'>Você recebeu este e-mail porque tem notificações ativas no .Ei.</p>";

        await SendEmailAsync(toEmail, subject, content);
    }


    /// <summary>
    /// Estrutura base do e-mail simulando o design moderno do app Pontuei.
    /// </summary>
    private string BuildHtmlTemplate(string title, string content)
    {
        return $@"
        <!DOCTYPE html>
        <html lang='pt-BR'>
        <head>
            <meta charset='UTF-8'>
            <meta name='viewport' content='width=device-width, initial-scale=1.0'>
            <title>{title}</title>
        </head>
        <body style='margin: 0; padding: 0; background-color: #f4f5f7; font-family: ""Helvetica Neue"", Helvetica, Arial, sans-serif; -webkit-font-smoothing: antialiased;'>
            <table width='100%' cellpadding='0' cellspacing='0' border='0' style='background-color: #f4f5f7; padding: 40px 10px;'>
                <tr>
                    <td align='center'>
                        <table width='100%' max-width='600' cellpadding='0' cellspacing='0' border='0' style='max-width: 600px; background-color: #ffffff; border-radius: 20px; overflow: hidden; box-shadow: 0 4px 15px rgba(0,0,0,0.05);'>
                            
                            <tr>
                                <td style='background-color: #1e8e3e; padding: 30px; text-align: center;'>
                                    <h1 style='color: #ffffff; margin: 0; font-size: 32px; font-weight: 800; letter-spacing: -1px;'>.ei</h1>
                                </td>
                            </tr>
                            
                            <tr>
                                <td style='padding: 40px 30px;'>
                                    {content}
                                </td>
                            </tr>
                            
                            <tr>
                                <td style='background-color: #fafafa; padding: 25px 30px; text-align: center; border-top: 1px solid #eeeeee;'>
                                    <p style='margin: 0; color: #999999; font-size: 13px;'>
                                        Nunca foi tão fácil colocar os pontos nos I's.
                                    </p>
                                    <p style='margin: 10px 0 0 0; color: #bbbbbb; font-size: 12px;'>
                                        &copy; {DateTime.Now.Year} .Ei. Todos os direitos reservados.<br>
                                        Por favor, não responda a este e-mail.
                                    </p>
                                </td>
                            </tr>
                            
                        </table>
                    </td>
                </tr>
            </table>
        </body>
        </html>";
    }
}