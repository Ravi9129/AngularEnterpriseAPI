using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        Task<string> GenerateRefreshTokenAsync(User user, string ipAddress);
        Task<bool> ValidateRefreshTokenAsync(string token);
        Task RevokeRefreshTokenAsync(string token, string ipAddress);
    }
}
