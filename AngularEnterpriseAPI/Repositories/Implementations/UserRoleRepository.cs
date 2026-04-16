using AngularEnterpriseAPI.Data;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Repositories.Implementations
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRoleRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<UserRoleAssignment> AssignRoleToUserAsync(int userId, int roleId)
        {
            var exists = await _dbContext.UserRoleAssignments.AnyAsync(a => a.UserId == userId && a.RoleId == roleId);
            if (exists)
                return await _dbContext.UserRoleAssignments.FirstAsync(a => a.UserId == userId && a.RoleId == roleId);

            var assignment = new UserRoleAssignment
            {
                UserId = userId,
                RoleId = roleId,
                AssignedAt = DateTime.UtcNow
            };

            _dbContext.UserRoleAssignments.Add(assignment);
            await _dbContext.SaveChangesAsync();
            return assignment;
        }

        public async Task<bool> RemoveRoleFromUserAsync(int userId, int roleId)
        {
            var assignment = await _dbContext.UserRoleAssignments.FirstOrDefaultAsync(a => a.UserId == userId && a.RoleId == roleId);
            if (assignment == null)
                return false;

            _dbContext.UserRoleAssignments.Remove(assignment);
            await _dbContext.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Role>> GetRolesByUserIdAsync(int userId)
        {
            return await _dbContext.UserRoleAssignments
                .Where(a => a.UserId == userId)
                .Select(a => a.Role)
                .ToListAsync();
        }
    }
}
