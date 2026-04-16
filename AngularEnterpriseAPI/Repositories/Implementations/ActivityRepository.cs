using AngularEnterpriseAPI.Data;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace AngularEnterpriseAPI.Repositories.Implementations
{
    public class ActivityRepository : Repository<UserActivity>, IActivityRepository
    {
        public ActivityRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(int userId, int limit = 50)
        {
            return await _dbSet
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 100)
        {
            return await _dbSet
                .OrderByDescending(a => a.Timestamp)
                .Take(limit)
                .Include(a => a.User)
                .ToListAsync();
        }

        public async Task<IEnumerable<UserActivity>> GetActivitiesByTypeAsync(string activityType, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _dbSet.Where(a => a.ActivityType == activityType);

            if (fromDate.HasValue)
                query = query.Where(a => a.Timestamp >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.Timestamp <= toDate.Value);

            return await query
                .OrderByDescending(a => a.Timestamp)
                .Include(a => a.User)
                .ToListAsync();
        }
    }
}
