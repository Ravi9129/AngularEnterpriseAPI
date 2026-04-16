using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using AngularEnterpriseAPI.Helpers;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly JwtSettings _jwtSettings;
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public TokenService(IOptions<JwtSettings> jwtSettings, IRefreshTokenRepository refreshTokenRepository)
        {
            _jwtSettings = jwtSettings.Value;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_jwtSettings.Secret);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.GivenName, user.FirstName),
                new Claim(ClaimTypes.Surname, user.LastName),
                new Claim(ClaimTypes.Role, user.Role.ToString()),
                new Claim("userId", user.Id.ToString()),
                new Claim("username", user.Username),
                new Claim("email", user.Email),
                new Claim("firstName", user.FirstName),
                new Claim("lastName", user.LastName),
                new Claim("role", user.Role.ToString())
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(_jwtSettings.AccessTokenExpirationMinutes),
                Issuer = _jwtSettings.Issuer,
                Audience = _jwtSettings.Audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public async Task<string> GenerateRefreshTokenAsync(User user, string ipAddress)
        {
            var refreshToken = new RefreshToken
            {
                Token = GenerateRandomToken(),
                UserId = user.Id,
                ExpiryDate = DateTime.UtcNow.AddDays(_jwtSettings.RefreshTokenExpirationDays),
                CreatedByIp = ipAddress,
                CreatedAt = DateTime.UtcNow
            };

            await _refreshTokenRepository.AddAsync(refreshToken);
            return refreshToken.Token;
        }

        public async Task<bool> ValidateRefreshTokenAsync(string token)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);
            return refreshToken != null && refreshToken.IsActive;
        }

        public async Task RevokeRefreshTokenAsync(string token, string ipAddress)
        {
            var refreshToken = await _refreshTokenRepository.GetByTokenAsync(token);

            if (refreshToken != null && !refreshToken.IsRevoked)
            {
                refreshToken.IsRevoked = true;
                refreshToken.RevokedAt = DateTime.UtcNow;
                refreshToken.RevokedByIp = ipAddress;
                await _refreshTokenRepository.UpdateAsync(refreshToken);
            }
        }

        private string GenerateRandomToken()
        {
            var randomNumber = new byte[32];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }
    }
}
