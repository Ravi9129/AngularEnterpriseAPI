using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.User;
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
    public class ProfileController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IActivityService _activityService;
        private readonly ILogger<ProfileController> _logger;

        public ProfileController(
            IUserService userService,
            IActivityService activityService,
            ILogger<ProfileController> logger)
        {
            _userService = userService;
            _activityService = activityService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var user = await _userService.GetUserByIdAsync(userId);

                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "Profile retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving profile");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving profile", 500));
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileDto updateProfileDto)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var user = await _userService.UpdateUserAsync(userId, new UpdateUserDto
                {
                    FirstName = updateProfileDto.FirstName,
                    LastName = updateProfileDto.LastName,
                    Email = updateProfileDto.Email
                });

                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse("User not found", 404));

                await _activityService.LogActivityAsync(userId, "ProfileUpdate", "User updated profile", GetIpAddress(), Request.Headers["User-Agent"]);

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "Profile updated successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating profile", 500));
            }
        }

        [HttpGet("activities")]
        public async Task<IActionResult> GetMyActivities([FromQuery] int limit = 50)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var activities = await _activityService.GetUserActivitiesAsync(userId, limit);

                return Ok(ApiResponse<IEnumerable<ActivityDto>>.SuccessResponse(activities, "Activities retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user activities");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving activities", 500));
            }
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }
}
