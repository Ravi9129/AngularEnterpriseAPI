using AngularEnterpriseAPI.DTOs.Auth;
using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.LoginAsync(request, ipAddress);

                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(
                    result,
                    "Login successful",
                    200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during login", 500));
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequestDto request)
        {
            try
            {
                var ipAddress = GetIpAddress();
                var result = await _authService.RefreshTokenAsync(request.RefreshToken, ipAddress);

                return Ok(ApiResponse<LoginResponseDto>.SuccessResponse(
                    result,
                    "Token refreshed successfully",
                    200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ApiResponse<object>.ErrorResponse(ex.Message, 401));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during token refresh", 500));
            }
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] string refreshToken)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var result = await _authService.LogoutAsync(userId, refreshToken);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Logged out successfully", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during logout");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred during logout", 500));
            }
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            try
            {
                var userId = int.Parse(User.FindFirst("userId")?.Value ?? "0");
                var result = await _authService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

                return Ok(ApiResponse<bool>.SuccessResponse(result, "Password changed successfully", 200));
            }
            catch (UnauthorizedAccessException ex)
            {
                return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message, 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while changing password", 500));
            }
        }

        private string GetIpAddress()
        {
            if (Request.Headers.ContainsKey("X-Forwarded-For"))
                return Request.Headers["X-Forwarded-For"].ToString();

            return HttpContext.Connection.RemoteIpAddress?.MapToIPv4().ToString() ?? "0.0.0.0";
        }
    }

    public class ChangePasswordRequest
    {
        public string CurrentPassword { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }
}
