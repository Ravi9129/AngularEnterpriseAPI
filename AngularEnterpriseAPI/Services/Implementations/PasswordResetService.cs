using System.Security.Cryptography;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class PasswordResetService : IPasswordResetService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPasswordResetRepository _passwordResetRepository;
        private readonly IEmailService _emailService;
        private readonly ILogger<PasswordResetService> _logger;

        public PasswordResetService(
            IUserRepository userRepository,
            IPasswordResetRepository passwordResetRepository,
            IEmailService emailService,
            ILogger<PasswordResetService> logger)
        {
            _userRepository = userRepository;
            _passwordResetRepository = passwordResetRepository;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendPasswordResetEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user == null)
            {
                // Don't reveal that the user doesn't exist
                _logger.LogWarning("Password reset requested for non-existent email: {Email}", email);
                return;
            }

            // Invalidate any existing reset tokens for this user
            await _passwordResetRepository.InvalidateUserTokensAsync(user.Id);

            // Generate new reset token
            var token = GenerateResetToken();
            var resetToken = new PasswordResetToken
            {
                UserId = user.Id,
                Token = token,
                ExpiryDate = DateTime.UtcNow.AddHours(1), // Token valid for 1 hour
                IsUsed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _passwordResetRepository.AddAsync(resetToken);

            // Send email
            await _emailService.SendPasswordResetEmailAsync(user.Email, token);
            _logger.LogInformation("Password reset email sent to {Email}", email);
        }

        public async Task<bool> ResetPasswordAsync(string token, string newPassword)
        {
            var resetToken = await _passwordResetRepository.GetByTokenAsync(token);

            if (resetToken == null || !resetToken.IsValid)
                return false;

            var user = resetToken.User;
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);

            // Mark token as used
            resetToken.IsUsed = true;
            await _passwordResetRepository.UpdateAsync(resetToken);

            // Invalidate all refresh tokens for security
            // This forces user to login again
            // await _refreshTokenRepository.RevokeUserTokensAsync(user.Id, "Password Reset");

            _logger.LogInformation("Password reset successful for user {Username}", user.Username);
            return true;
        }

        public async Task<bool> ValidateResetTokenAsync(string token)
        {
            var resetToken = await _passwordResetRepository.GetByTokenAsync(token);
            return resetToken != null && resetToken.IsValid;
        }

        private string GenerateResetToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
