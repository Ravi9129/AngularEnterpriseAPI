using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AngularEnterpriseAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PasswordResetController : ControllerBase
    {
        private readonly IPasswordResetService _passwordResetService;
        private readonly ILogger<PasswordResetController> _logger;

        public PasswordResetController(IPasswordResetService passwordResetService, ILogger<PasswordResetController> logger)
        {
            _passwordResetService = passwordResetService;
            _logger = logger;
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                await _passwordResetService.SendPasswordResetEmailAsync(request.Email);
                return Ok(ApiResponse<object>.SuccessResponse(null, "If the email exists, a password reset link has been sent", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forgot password");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while processing your request", 500));
            }
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                var result = await _passwordResetService.ResetPasswordAsync(request.Token, request.NewPassword);

                if (result)
                    return Ok(ApiResponse<object>.SuccessResponse(null, "Password reset successfully", 200));
                else
                    return BadRequest(ApiResponse<object>.ErrorResponse("Invalid or expired token", 400));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while resetting password", 500));
            }
        }

        [HttpPost("validate-token")]
        public async Task<IActionResult> ValidateToken([FromBody] ValidateTokenRequest request)
        {
            try
            {
                var isValid = await _passwordResetService.ValidateResetTokenAsync(request.Token);
                return Ok(ApiResponse<bool>.SuccessResponse(isValid, "Token validation completed", 200));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating token");
                return StatusCode(500, ApiResponse<object>.ErrorResponse("An error occurred while validating token", 500));
            }
        }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class ValidateTokenRequest
    {
        public string Token { get; set; } = string.Empty;
    }
}
