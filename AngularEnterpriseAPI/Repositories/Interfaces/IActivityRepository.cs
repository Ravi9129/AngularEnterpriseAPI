using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Repositories.Interfaces
{
    public interface IActivityRepository : IRepository<UserActivity>
    {
        Task<IEnumerable<UserActivity>> GetUserActivitiesAsync(int userId, int limit = 50);
        Task<IEnumerable<UserActivity>> GetRecentActivitiesAsync(int limit = 100);
        Task<IEnumerable<UserActivity>> GetActivitiesByTypeAsync(string activityType, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
