using AngularEnterpriseAPI.Data;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Repositories.Implementations
{
    public class UserRepository : Repository<User>, IUserRepository
    {
        public UserRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _dbSet
                .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);
        }

        public async Task<IEnumerable<User>> GetUsersByRoleAsync(string role)
        {
            return await _dbSet
                .Where(u => u.Role.ToString() == role && u.IsActive)
                .ToListAsync();
        }

        public async Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(
            int pageNumber,
            int pageSize,
            string? filter = null,
            string? sortBy = null)
        {
            var query = _dbSet.Where(u => u.IsActive);

            // Apply filter
            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(u =>
                    u.Username.Contains(filter) ||
                    u.Email.Contains(filter) ||
                    u.FirstName.Contains(filter) ||
                    u.LastName.Contains(filter));
            }

            // Get total count
            var totalCount = await query.CountAsync();

            // Apply sorting
            if (!string.IsNullOrWhiteSpace(sortBy))
            {
                query = sortBy.ToLower() switch
                {
                    "username" => query.OrderBy(u => u.Username),
                    "username_desc" => query.OrderByDescending(u => u.Username),
                    "email" => query.OrderBy(u => u.Email),
                    "email_desc" => query.OrderByDescending(u => u.Email),
                    "firstname" => query.OrderBy(u => u.FirstName),
                    "firstname_desc" => query.OrderByDescending(u => u.FirstName),
                    "lastname" => query.OrderBy(u => u.LastName),
                    "lastname_desc" => query.OrderByDescending(u => u.LastName),
                    "createdat" => query.OrderBy(u => u.CreatedAt),
                    "createdat_desc" => query.OrderByDescending(u => u.CreatedAt),
                    _ => query.OrderBy(u => u.Id)
                };
            }
            else
            {
                query = query.OrderByDescending(u => u.CreatedAt);
            }

            // Apply pagination
            var users = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (users, totalCount);
        }
    }
}
