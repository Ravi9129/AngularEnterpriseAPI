using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.Permission;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class PermissionsController : ControllerBase
    {
        private readonly IPermissionService _permissionService;
        private readonly IActivityService _activityService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IPermissionService permissionService, IActivityService activityService, ILogger<PermissionsController> logger)
        {
            _permissionService = permissionService;
            _activityService = activityService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePermission([FromBody] CreatePermissionDto dto)
        {
            try
            {
                var perm = await _permissionService.CreatePermissionAsync(dto);
                return CreatedAtAction(nameof(GetAll), new { id = perm.Id }, ApiResponse<PermissionResponseDto>.SuccessResponse(perm, "Permission created", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, 409));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating permission");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating permission", 500));
            }
        }

        [HttpPost("assign/user")]
        public async Task<IActionResult> AssignToUser([FromBody] AssignPermissionDto dto)
        {
            try
            {
                var result = await _permissionService.AssignPermissionToUserAsync(dto);

                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "AssignPermission", $"Assigned permission to user", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Permission assigned to user", 200));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission to user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while assigning permission", 500));
            }
        }

        [HttpPost("assign/role")]
        public async Task<IActionResult> AssignToRole([FromBody] AssignPermissionDto dto)
        {
            try
            {
                var result = await _permissionService.AssignPermissionToRoleAsync(dto);

                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "AssignPermission", $"Assigned permission to role '{dto.RoleName}'", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Permission assigned to role", 200));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ApiResponse<object>.ErrorResponse(ex.Message, 404));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning permission to role");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while assigning permission to role", 500));
            }
        }

        [HttpPost("remove/user")]
        public async Task<IActionResult> RemoveFromUser([FromBody] AssignPermissionDto dto)
        {
            try
            {
                var result = await _permissionService.RemovePermissionFromUserAsync(dto);

                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "RemovePermission", $"Removed permission from user", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Permission removed from user", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission from user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while removing permission", 500));
            }
        }

        [HttpPost("remove/role")]
        public async Task<IActionResult> RemoveFromRole([FromBody] AssignPermissionDto dto)
        {
            try
            {
                var result = await _permissionService.RemovePermissionFromRoleAsync(dto);

                var actorId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                await _activityService.LogActivityAsync(actorId, "RemovePermission", $"Removed permission from role '{dto.RoleName}'", Request.Headers["X-Forwarded-For"].FirstOrDefault() ?? HttpContext.Connection.RemoteIpAddress?.ToString(), Request.Headers["User-Agent"].ToString());

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Permission removed from role", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing permission from role");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while removing permission from role", 500));
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var perms = await _permissionService.GetAllPermissionsAsync();
                return Ok(ApiResponse<IEnumerable<PermissionResponseDto>>.SuccessResponse(perms, "Permissions retrieved", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving permissions");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving permissions", 500));
            }
        }
    }
}
