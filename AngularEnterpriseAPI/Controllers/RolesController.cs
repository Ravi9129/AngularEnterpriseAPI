using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.Role;
using AngularEnterpriseAPI.Services.Interfaces;
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

        public RolesController(IRoleService roleService, ILogger<RolesController> logger)
        {
            _roleService = roleService;
            _logger = logger;
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
                var result = await _roleService.AssignRoleToUserAsync(dto);
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
                var result = await _roleService.RemoveRoleFromUserAsync(dto);
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
