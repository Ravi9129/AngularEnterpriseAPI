namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IPasswordResetService
    {
        Task SendPasswordResetEmailAsync(string email);
        Task<bool> ResetPasswordAsync(string token, string newPassword);
        Task<bool> ValidateResetTokenAsync(string token);
    }
}
