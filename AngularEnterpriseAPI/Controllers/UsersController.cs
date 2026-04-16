using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.User;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(IUserService userService, ILogger<UsersController> logger)
        {
            _userService = userService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetUsers(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] string? filter = null,
            [FromQuery] string? sortBy = null)
        {
            try
            {
                var users = await _userService.GetPagedUsersAsync(page, pageSize, filter, sortBy);
                return Ok(ApiResponse<PagedResponse<UserResponseDto>>.SuccessResponse(
                    users,
                    "Users retrieved successfully",
                    200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving users");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving users", 500));
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userService.GetUserByIdAsync(id);

                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"User with id {id} not found", 404));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user", 500));
            }
        }

        [HttpGet("username/{username}")]
        public async Task<IActionResult> GetUserByUsername(string username)
        {
            try
            {
                var user = await _userService.GetUserByUsernameAsync(username);

                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"User with username {username} not found", 404));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User retrieved successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user by username {Username}", username);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while retrieving user", 500));
            }
        }

        [HttpPost]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserDto createUserDto)
        {
            try
            {
                var user = await _userService.CreateUserAsync(createUserDto);
                return CreatedAtAction(nameof(GetUserById), new { id = user.Id },
                    ApiResponse<UserResponseDto>.SuccessResponse(user, "User created successfully", 201));
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ApiResponse<object>.ErrorResponse(ex.Message, 409));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating user");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while creating user", 500));
            }
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserDto updateUserDto)
        {
            try
            {
                var user = await _userService.UpdateUserAsync(id, updateUserDto);

                if (user == null)
                    return NotFound(ApiResponse<object>.ErrorResponse($"User with id {id} not found", 404));

                return Ok(ApiResponse<UserResponseDto>.SuccessResponse(user, "User updated successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while updating user", 500));
            }
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "ADMIN")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(id);

                if (!result)
                    return NotFound(ApiResponse<object>.ErrorResponse($"User with id {id} not found", 404));

                return Ok(ApiResponse<bool>.SuccessResponse(true, "User deleted successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user {UserId}", id);
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while deleting user", 500));
            }
        }
    }
}
