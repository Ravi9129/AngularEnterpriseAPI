using AngularEnterpriseAPI.DTOs.User;

namespace AngularEnterpriseAPI.DTOs.Auth
{
    public class LoginResponseDto
    {
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public UserResponseDto User { get; set; } = null!;
    }
}
