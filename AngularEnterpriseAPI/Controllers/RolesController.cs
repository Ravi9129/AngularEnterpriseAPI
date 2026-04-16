using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.Role;
using AngularEnterpriseAPI.Services.Interfaces;
using AngularEnterpriseAPI.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class RolesController : ControllerBase
    {
        private readonly IRoleService _roleService;
        private readonly ILogger<RolesController> _logger;
        private readonly IUserRepository _userRepository;
        private readonly IActivityService _activityService;

        public RolesController(IRoleService roleService, ILogger<RolesController> logger, IUserRepository userRepository, IActivityService activityService)
        {
            _roleService = roleService;
            _logger = logger;
            _userRepository = userRepository;
            _activityService = activityService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleDto dto)
        {
            try
            {
                var role = await _roleService.CreateRoleAsync(dto);
                return CreatedAtAction(nameof(GetAllRoles), new { id = role.Id }, ApiResponse<RoleResponseDto>.SuccessResponse(role, "Role created", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, 409));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating role");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating role", 500));
            }
        }

        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                // Resolve user identifier (support Id / Username / Email)
                int targetUserId = 0;

                if (dto.UserId.HasValue && dto.UserId.Value > 0)
                {
                    targetUserId = dto.UserId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var user = await _userRepository.GetByUsernameAsync(dto.Username);
                    if (user == null)
                        return NotFound(ApiResponse<object>.ErrorResponse("Target user not found", 404));

                    targetUserId = user.Id;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var user = await _userRepository.GetByEmailAsync(dto.Email);
                    if (user == null)
                        return NotFound(ApiResponse<object>.ErrorResponse("Target user not found", 404));

                    targetUserId = user.Id;
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Provide userId, username or email to identify the user", 400));
                }

                // set resolved id on DTO and call service
                dto.UserId = targetUserId;
                var result = await _roleService.AssignRoleToUserAsync(dto);

                // Log admin action
                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "AssignRole", $"Assigned role '{dto.RoleName}' to user '{targetUserId}'", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Role assigned to user", 200));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while assigning role", 500));
            }
        }

        [HttpPost("remove")]
        public async Task<IActionResult> RemoveRole([FromBody] AssignRoleDto dto)
        {
            try
            {
                int targetUserId = 0;

                if (dto.UserId.HasValue && dto.UserId.Value > 0)
                {
                    targetUserId = dto.UserId.Value;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Username))
                {
                    var user = await _userRepository.GetByUsernameAsync(dto.Username);
                    if (user == null)
                        return NotFound(ApiResponse<object>.ErrorResponse("Target user not found", 404));

                    targetUserId = user.Id;
                }
                else if (!string.IsNullOrWhiteSpace(dto.Email))
                {
                    var user = await _userRepository.GetByEmailAsync(dto.Email);
                    if (user == null)
                        return NotFound(ApiResponse<object>.ErrorResponse("Target user not found", 404));

                    targetUserId = user.Id;
                }
                else
                {
                    return BadRequest(ApiResponse<object>.ErrorResponse("Provide userId, username or email to identify the user", 400));
                }

                dto.UserId = targetUserId;
                var result = await _roleService.RemoveRoleFromUserAsync(dto);

                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "RemoveRole", $"Removed role '{dto.RoleName}' from user '{targetUserId}'", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Role removed from user", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing role");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while removing role", 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleService.GetAllRolesAsync();
            return Ok(ApiResponse<IEnumerable<RoleResponseDto>>.SuccessResponse(roles, "Roles retrieved", 200));
        }
    }
}
