using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.Dashboard;
using AngularEnterpriseAPI.DTOs.Activity;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class DashboardController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly ILogger<DashboardController> _logger;

        public DashboardController(
            IUserService userService,
            IActivityService activityService,
            ILogger<DashboardController> logger)
        {
            _userService = userService;
            _activityService = activityService;
            _logger = logger;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                var stats = new DashboardStatsDto
                {
                    TotalUsers = await _userService.GetTotalUsersCountAsync(),
                    ActiveUsers = await _userService.GetActiveUsersCountAsync(),
                    NewUsersThisMonth = await _userService.GetNewUsersCountAsync(DateTime.UtcNow.AddMonths(-1)),
                    UsersByRole = await _userService.GetUsersCountByRoleAsync()
                };

                return Ok(ApiResponse<DashboardStatsDto>.SuccessResponse(stats, "Dashboard stats retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving dashboard stats");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving dashboard stats", 500));
            }
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetRecentActivities([FromQuery] int limit = 10)
        {
            try
            {
                var activities = await _activityService.GetRecentActivitiesAsync(limit);
                return Ok(ApiResponse<IEnumerable<ActivityDto>>.SuccessResponse(activities, "Recent activities retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving recent activities");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving activities", 500));
            }
        }

        [HttpGet("user-stats")]
        public async Task<IActionResult> GetUserStats()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var userStats = new UserStatsDto
                {
                    TotalUsers = await _userService.GetTotalUsersCountAsync(),
                    ActiveUsers = await _userService.GetActiveUsersCountAsync(),
                    InactiveUsers = await _userService.GetTotalUsersCountAsync() - await _userService.GetActiveUsersCountAsync(),
                    AdminCount = (await _userService.GetUsersCountByRoleAsync()).GetValueOrDefault("ADMIN", 0),
                    ManagerCount = (await _userService.GetUsersCountByRoleAsync()).GetValueOrDefault("MANAGER", 0),
                    RegularUserCount = (await _userService.GetUsersCountByRoleAsync()).GetValueOrDefault("USER", 0),
                    UserGrowthPercentage = await CalculateUserGrowthPercentage()
                };

                return Ok(ApiResponse<UserStatsDto>.SuccessResponse(userStats, "User statistics retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user stats");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user statistics", 500));
            }
        }

        private async Task<double> CalculateUserGrowthPercentage()
        {
            var lastMonth = DateTime.UtcNow.AddMonths(-1);
            var twoMonthsAgo = DateTime.UtcNow.AddMonths(-2);

            var lastMonthCount = await _userService.GetNewUsersCountAsync(lastMonth);
            var previousMonthCount = await _userService.GetNewUsersCountAsync(twoMonthsAgo);

            if (previousMonthCount == 0) return 0;

            return Math.Round(((double)(lastMonthCount - previousMonthCount) / previousMonthCount) * 100, 2);
        }
    }
}
