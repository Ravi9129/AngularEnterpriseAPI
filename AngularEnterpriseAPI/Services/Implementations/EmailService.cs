using System.Net;
using System.Net.Mail;
using AngularEnterpriseAPI.Services.Interfaces;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, bool isHtml = false)
        {
            try
            {
                var smtpSettings = _configuration.GetSection("SmtpSettings");
                var fromEmail = smtpSettings["FromEmail"] ?? "noreply@enterprise.com";
                var smtpHost = smtpSettings["Host"] ?? "smtp.gmail.com";
                var smtpPort = int.Parse(smtpSettings["Port"] ?? "587");
                var smtpUser = smtpSettings["Username"];
                var smtpPass = smtpSettings["Password"];

                using var client = new SmtpClient(smtpHost, smtpPort);
                client.EnableSsl = true;
                client.Credentials = new NetworkCredential(smtpUser, smtpPass);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = isHtml
                };
                mailMessage.To.Add(to);

                await client.SendMailAsync(mailMessage);
                _logger.LogInformation("Email sent successfully to {To}", to);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email to {To}", to);
                throw;
            }
        }

        public async Task SendWelcomeEmailAsync(string to, string username)
        {
            var subject = "Welcome to Angular Enterprise App!";
            var body = $@"
                <h1>Welcome {username}!</h1>
                <p>Thank you for joining Angular Enterprise App. We're excited to have you on board!</p>
                <p>Best regards,<br/>The Enterprise Team</p>";

            await SendEmailAsync(to, subject, body, true);
        }

        public async Task SendPasswordResetEmailAsync(string to, string resetToken)
        {
            var resetLink = $"https://localhost:4200/reset-password?token={resetToken}";
            var subject = "Password Reset Request";
            var body = $@"
                <h1>Password Reset Request</h1>
                <p>Click the link below to reset your password:</p>
                <a href='{resetLink}'>{resetLink}</a>
                <p>This link will expire in 1 hour.</p>
                <p>If you didn't request this, please ignore this email.</p>";

            await SendEmailAsync(to, subject, body, true);
        }
    }
}
