
using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.InterviewSchedule;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class InterviewScheduleController : ControllerBase
    {
        private readonly IInterviewScheduleService _interviewScheduleService;

        public InterviewScheduleController(
            IInterviewScheduleService interviewScheduleService)
        {
            _interviewScheduleService = interviewScheduleService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "User ID not found.");
        }

        private string GetUserRole()
        {
            return User.FindFirstValue(ClaimTypes.Role)
                ?? throw new UnauthorizedAccessException(
                    "User role not found.");
        }

        // ==========================================
        // POST: api/applications/{applicationId}/interviews
        // ==========================================

        [HttpPost("applications/{applicationId:int}/interviews")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> CreateInterviewSchedule(
            int applicationId,
            [FromBody] InterviewScheduleCreateDto dto)
        {
            try
            {
                var result =
                    await _interviewScheduleService.CreateInterviewScheduleAsync(
                        applicationId,
                        dto,
                        GetUserId());

                return CreatedAtAction(
                    nameof(GetInterviewSchedules),
                    new { applicationId },
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // GET: api/applications/{applicationId}/interviews
        // ==========================================

        [HttpGet("applications/{applicationId:int}/interviews")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,CompanyHR,Student")]
        public async Task<IActionResult> GetInterviewSchedules(
            int applicationId)
        {
            try
            {
                var result =
                    await _interviewScheduleService
                        .GetInterviewSchedulesByApplicationAsync(
                            applicationId,
                            GetUserId(),
                            GetUserRole());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // PUT: api/interviews/{id}
        // ==========================================

        [HttpPut("interviews/{id:int}")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> UpdateInterviewSchedule(
            int id,
            [FromBody] InterviewScheduleUpdateDto dto)
        {
            try
            {
                var result =
                    await _interviewScheduleService
                        .UpdateInterviewScheduleAsync(
                            id,
                            dto,
                            GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // ==========================================
        // PUT: api/interviews/{id}/feedback
        // ==========================================

        [HttpPut("interviews/{id:int}/feedback")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> UpdateInterviewFeedback(
            int id,
            [FromBody] string feedback)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(feedback))
                    return BadRequest(new
                    {
                        message = "Feedback is required."
                    });

                var result =
                    await _interviewScheduleService
                        .UpdateInterviewFeedbackAsync(
                            id,
                            feedback,
                            GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
