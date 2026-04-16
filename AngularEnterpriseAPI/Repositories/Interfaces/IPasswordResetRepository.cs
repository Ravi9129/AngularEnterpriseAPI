using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Repositories.Interfaces
{
    public interface IPasswordResetRepository : IRepository<PasswordResetToken>
    {
        Task<PasswordResetToken?> GetByTokenAsync(string token);
        Task InvalidateUserTokensAsync(int userId);
    }
}
