
using CampusPlacementManagementAPI.DTOs.Auth;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/auth/login
        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var result = await _authService.LoginAsync(dto);

            if (result == null)
            {
                return Unauthorized(new
                {
                    message = "Invalid email or password, or account is not approved."
                });
            }

            return Ok(result);
        }

        // PUT: api/auth/change-password
        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword(
            ChangePasswordDto dto)
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized();

            var result = await _authService.ChangePasswordAsync(
                userId,
                dto);

            if (!result)
            {
                return BadRequest(new
                {
                    message = "Password change failed. Check your current password and password requirements."
                });
            }

            return Ok(new
            {
                message = "Password changed successfully."
            });
        }

        // PUT: api/auth/admin-reset-password
        [HttpPut("admin-reset-password")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AdminResetPassword(
            AdminResetPasswordDto dto)
        {
            var result = await _authService.ResetUserPasswordAsync(dto);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "User with the given email was not found."
                });
            }

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Password reset failed.",
                    errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                message = "User password reset successfully."
            });
        }

        // PUT: api/auth/assign-student-role
        [HttpPut("assign-student-role")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> AssignStudentRole(
            AdminAssignStudentRoleDto dto)
        {
            var result = await _authService.AssignStudentRoleAsync(
                dto.Email);

            if (result == null)
            {
                return NotFound(new
                {
                    message = "User with the given email was not found."
                });
            }

            if (!result.Succeeded)
            {
                return BadRequest(new
                {
                    message = "Student role assignment failed.",
                    errors = result.Errors.Select(error => error.Description)
                });
            }

            return Ok(new
            {
                message = "Student role assigned successfully."
            });
        }
    }
}
