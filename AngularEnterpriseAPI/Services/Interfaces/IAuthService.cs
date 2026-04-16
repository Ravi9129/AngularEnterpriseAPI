using AngularEnterpriseAPI.DTOs.Auth;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress);
        Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> LogoutAsync(int userId, string refreshToken);
        Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
        Task<bool> ValidateTokenAsync(string token);
    }
}
