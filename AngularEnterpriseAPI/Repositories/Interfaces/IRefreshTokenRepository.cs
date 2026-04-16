using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Repositories.Interfaces
{
    public interface IRefreshTokenRepository : IRepository<RefreshToken>
    {
        Task<RefreshToken?> GetByTokenAsync(string token);
        Task RevokeUserTokensAsync(int userId, string revokedByIp);
        Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int userId);
    }
}
