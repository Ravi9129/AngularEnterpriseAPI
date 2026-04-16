using AngularEnterpriseAPI.Data;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Repositories.Implementations
{
    public class PasswordResetRepository : Repository<PasswordResetToken>, IPasswordResetRepository
    {
        public PasswordResetRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<PasswordResetToken?> GetByTokenAsync(string token)
        {
            return await _dbSet
                .Include(prt => prt.User)
                .FirstOrDefaultAsync(prt => prt.Token == token);
        }

        public async Task InvalidateUserTokensAsync(int userId)
        {
            var tokens = await _dbSet
                .Where(prt => prt.UserId == userId && !prt.IsUsed)
                .ToListAsync();

            foreach (var token in tokens)
            {
                token.IsUsed = true;
            }

            await _context.SaveChangesAsync();
        }
    }
}
