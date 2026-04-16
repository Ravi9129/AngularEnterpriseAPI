using AngularEnterpriseAPI.Data;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Repositories.Implementations
{
    public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
    {
        public RefreshTokenRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }

        public async Task RevokeUserTokensAsync(int userId, string revokedByIp)
        {
            var tokens = await _dbSet
                .Where(rt => rt.UserId == userId && !rt.IsRevoked)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsRevoked = true;
                token.RevokedAt = DateTime.UtcNow;
                token.RevokedByIp = revokedByIp;
            }

            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RefreshToken>> GetUserActiveTokensAsync(int userId)
        {
            return await _dbSet
                .Where(rt => rt.UserId == userId && rt.IsActive)
                .ToListAsync();
        }
    }
}
