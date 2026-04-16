using AngularEnterpriseAPI.DTOs.Activity;
using System.Collections.Generic;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IActivityService
    {
        Task LogActivityAsync(int userId, string activityType, string description, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null);
        Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(int userId, int limit = 50);
        Task<IEnumerable<ActivityDto>> GetRecentActivitiesAsync(int limit = 100);
    }
}
