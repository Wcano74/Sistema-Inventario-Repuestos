using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace SistemaInventario.Services
{
    public interface IEmailService
    {
        Task<bool> SendEmailAsync(string to, string subject, string body);
    }

    public class EmailService : IEmailService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IServiceProvider serviceProvider, ILogger<EmailService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        public async Task<bool> SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using var scope = _serviceProvider.CreateScope();
                var configService = scope.ServiceProvider.GetRequiredService<IConfigurationService>();

                var smtpHost = await configService.GetConfigurationAsync("Smtp_Host", "");
                var smtpPortStr = await configService.GetConfigurationAsync("Smtp_Port", "587");
                var smtpUser = await configService.GetConfigurationAsync("Smtp_User", "");
                var smtpPassword = await configService.GetConfigurationAsync("Smtp_Password", "");
                var smtpFrom = await configService.GetConfigurationAsync("Smtp_FromEmail", "");
                var smtpSsl = (await configService.GetConfigurationAsync("Smtp_UseSsl", "true")) == "true";

                if (string.IsNullOrWhiteSpace(smtpHost) || string.IsNullOrWhiteSpace(smtpUser) || string.IsNullOrWhiteSpace(smtpFrom))
                {
                    _logger.LogWarning("No se puede enviar correo: configuración SMTP incompleta.");
                    return false;
                }

                if (!int.TryParse(smtpPortStr, out var smtpPort)) smtpPort = 587;

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    Credentials = new NetworkCredential(smtpUser, smtpPassword),
                    EnableSsl = smtpSsl,
                    Timeout = 30000 // 30 segundos
                };

                var message = new MailMessage
                {
                    From = new MailAddress(smtpFrom, "Sistema de Inventario"),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = true
                };
                message.To.Add(to);

                await client.SendMailAsync(message);
                _logger.LogInformation("Correo enviado exitosamente a {To}", to);
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar correo a {To}", to);
                return false;
            }
        }
    }
}
