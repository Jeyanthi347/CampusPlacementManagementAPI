
using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.RecruitmentDrive;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/recruitment-drives")]
    [ApiController]
    [Authorize]
    public class RecruitmentDriveController : ControllerBase
    {
        private readonly IRecruitmentDriveService _driveService;

        public RecruitmentDriveController(
            IRecruitmentDriveService driveService)
        {
            _driveService = driveService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "User ID not found in token.");
        }

        private string GetUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role) ?? "";
        }

        // Company HR and SuperAdmin create a recruitment drive
        [HttpPost]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> CreateDrive(
            [FromBody] RecruitmentDriveCreateDto dto)
        {
            try
            {
                var result = await _driveService.CreateDriveAsync(
                    dto, GetUserId(), GetUserRole());

                return CreatedAtAction(
                    nameof(GetDriveById),
                    new { id = result.DriveId },
                    result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get all drives based on logged-in user's role
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,CompanyHR,Student")]
        public async Task<IActionResult> GetAllDrives()
        {
            try
            {
                var result = await _driveService.GetAllDrivesAsync(
                    GetUserId(), GetUserRole());

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get a recruitment drive by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,CompanyHR,Student")]
        public async Task<IActionResult> GetDriveById(int id)
        {
            try
            {
                var result = await _driveService.GetDriveByIdAsync(
                    id, GetUserId(), GetUserRole());

                if (result == null)
                {
                    return NotFound(new
                    {
                        message = "Recruitment drive not found."
                    });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Company HR and SuperAdmin update a recruitment drive
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> UpdateDrive(
            int id, [FromBody] RecruitmentDriveUpdateDto dto)
        {
            try
            {
                var result = await _driveService.UpdateDriveAsync(
                    id, dto, GetUserId(), GetUserRole());

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Company HR and SuperAdmin cancel a recruitment drive
        [HttpPut("{id:int}/cancel")]
        [Authorize(Roles = "SuperAdmin,CompanyHR")]
        public async Task<IActionResult> CancelDrive(int id)
        {
            try
            {
                var result = await _driveService.CancelDriveAsync(
                    id, GetUserId(), GetUserRole());

                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
