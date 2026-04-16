using System;
using System.Collections.Generic;
using AngularEnterpriseAPI.DTOs.Activity;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using AutoMapper;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class ActivityService : IActivityService
    {
        private readonly IActivityRepository _activityRepository;
        private readonly IMapper _mapper;

        public ActivityService(IActivityRepository activityRepository, IMapper mapper)
        {
            _activityRepository = activityRepository;
            _mapper = mapper;
        }

        public async Task LogActivityAsync(int userId, string activityType, string description, string? ipAddress = null, string? userAgent = null, Dictionary<string, object>? metadata = null)
        {
            var activity = new UserActivity
            {
                UserId = userId,
                ActivityType = activityType,
                Description = description,
                IpAddress = ipAddress,
                UserAgent = userAgent,
                Metadata = metadata,
                Timestamp = DateTime.UtcNow
            };

            await _activityRepository.AddAsync(activity);
        }

        public async Task<IEnumerable<ActivityDto>> GetUserActivitiesAsync(int userId, int limit = 50)
        {
            var activities = await _activityRepository.GetUserActivitiesAsync(userId, limit);
            return _mapper.Map<IEnumerable<ActivityDto>>(activities);
        }

        public async Task<IEnumerable<ActivityDto>> GetRecentActivitiesAsync(int limit = 100)
        {
            var activities = await _activityRepository.GetRecentActivitiesAsync(limit);
            return _mapper.Map<IEnumerable<ActivityDto>>(activities);
        }
    }
}
