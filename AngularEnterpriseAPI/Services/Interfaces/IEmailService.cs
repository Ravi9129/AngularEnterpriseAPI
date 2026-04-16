namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, bool isHtml = false);
        Task SendWelcomeEmailAsync(string to, string username);
        Task SendPasswordResetEmailAsync(string to, string resetToken);
    }
}
