using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SSBJr.WebAuth.Services;

public interface IEmailService
{
    Task SendTwoFactorCodeAsync(string email, string code);
    Task SendWelcomeEmailAsync(string email, string userName);
}

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendTwoFactorCodeAsync(string email, string code)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("Email");
            using (var client = new SmtpClient(smtpSettings["SmtpServer"]))
            {
                client.Port = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                client.EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                client.Credentials = new NetworkCredential(
                    smtpSettings["FromEmail"],
                    smtpSettings["SmtpPassword"]
                );

                var message = new MailMessage(
                    smtpSettings["FromEmail"],
                    email
                )
                {
                    Subject = "Seu código de autenticação de dois fatores",
                    Body = $@"
                        <h2>Código de Autenticação</h2>
                        <p>Seu código de autenticação de dois fatores é:</p>
                        <h1>{code}</h1>
                        <p>Este código expira em 15 minutos.</p>
                        <p><strong>Nunca compartilhe este código com ninguém.</strong></p>
                    ",
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"2FA code sent to {email}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending email to {email}: {ex.Message}");
            throw;
        }
    }

    public async Task SendWelcomeEmailAsync(string email, string userName)
    {
        try
        {
            var smtpSettings = _configuration.GetSection("Email");
            using (var client = new SmtpClient(smtpSettings["SmtpServer"]))
            {
                client.Port = int.Parse(smtpSettings["SmtpPort"] ?? "587");
                client.EnableSsl = bool.Parse(smtpSettings["EnableSsl"] ?? "true");
                client.Credentials = new NetworkCredential(
                    smtpSettings["FromEmail"],
                    smtpSettings["SmtpPassword"]
                );

                var message = new MailMessage(
                    smtpSettings["FromEmail"],
                    email
                )
                {
                    Subject = "Bem-vindo ao Sistema",
                    Body = $@"
                        <h2>Bem-vindo, {userName}!</h2>
                        <p>Sua conta foi criada com sucesso.</p>
                        <p>Você pode acessar o sistema com suas credenciais.</p>
                    ",
                    IsBodyHtml = true
                };

                await client.SendMailAsync(message);
                _logger.LogInformation($"Welcome email sent to {email}");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error sending welcome email to {email}: {ex.Message}");
            throw;
        }
    }
}
