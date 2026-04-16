using AngularEnterpriseAPI.DTOs.Auth;
using AngularEnterpriseAPI.DTOs.User;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using AutoMapper;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;

        public AuthService(
            IUserRepository userRepository,
            ITokenService tokenService,
            IMapper mapper)
        {
            _userRepository = userRepository;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<LoginResponseDto> RegisterAsync(AngularEnterpriseAPI.DTOs.User.CreateUserDto request, string ipAddress)
        {
            // Check if user exists
            var exists = await _userRepository.ExistsAsync(u => u.Username == request.Username || u.Email == request.Email);
            if (exists)
                throw new InvalidOperationException("Username or email already exists");

            var user = _mapper.Map<AngularEnterpriseAPI.Models.Entities.User>(request);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var created = await _userRepository.AddAsync(user);

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(created);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(created, ipAddress);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserResponseDto>(created)
            };
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request, string ipAddress)
        {
            // Find user by username
            var user = await _userRepository.GetByUsernameAsync(request.Username);

            if (user == null)
                throw new UnauthorizedAccessException("Invalid username or password");

            // Verify password
            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                throw new UnauthorizedAccessException("Invalid username or password");

            // Check if user is active
            if (!user.IsActive)
                throw new UnauthorizedAccessException("Account is deactivated");

            // Generate tokens
            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshTokenAsync(user, ipAddress);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                User = _mapper.Map<UserResponseDto>(user)
            };
        }

        public async Task<LoginResponseDto> RefreshTokenAsync(string refreshToken, string ipAddress)
        {
            // Validate refresh token
            var isValid = await _tokenService.ValidateRefreshTokenAsync(refreshToken);

            if (!isValid)
                throw new UnauthorizedAccessException("Invalid refresh token");

            // Get the refresh token entity
            // Note: You would need a method to get refresh token by token string
            // For brevity, we're using a simple approach here

            // In a real implementation, you would:
            // 1. Get the refresh token from database
            // 2. Get the associated user
            // 3. Generate new tokens
            // 4. Revoke old refresh token
            // 5. Return new tokens

            throw new NotImplementedException("Refresh token implementation requires additional repository methods");
        }

        public async Task<bool> LogoutAsync(int userId, string refreshToken)
        {
            await _tokenService.RevokeRefreshTokenAsync(refreshToken, string.Empty);
            return true;
        }

        public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
                throw new UnauthorizedAccessException("Current password is incorrect");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> ValidateTokenAsync(string token)
        {
            // Implement token validation logic
            // This would typically use JWT validation
            return true;
        }
    }
}
