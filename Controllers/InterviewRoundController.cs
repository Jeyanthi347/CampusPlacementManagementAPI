
using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.InterviewRound;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api")]
    [ApiController]
    public class InterviewRoundController : ControllerBase
    {
        private readonly IInterviewRoundService _interviewRoundService;

        public InterviewRoundController(
            IInterviewRoundService interviewRoundService)
        {
            _interviewRoundService = interviewRoundService;
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

        // POST: api/jobs/{jobId}/interview-rounds
        [HttpPost("jobs/{jobId:int}/interview-rounds")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> CreateInterviewRound(
            int jobId,
            [FromBody] InterviewRoundCreateDto dto)
        {
            try
            {
                var result =
                    await _interviewRoundService.CreateInterviewRoundAsync(
                        jobId, dto, GetUserId());

                return CreatedAtAction(
                    nameof(GetInterviewRounds),
                    new { jobId },
                    result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/jobs/{jobId}/interview-rounds
        [HttpGet("jobs/{jobId:int}/interview-rounds")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,CompanyHR,Student")]
        public async Task<IActionResult> GetInterviewRounds(int jobId)
        {
            try
            {
                var result =
                    await _interviewRoundService.GetInterviewRoundsByJobAsync(
                        jobId, GetUserId(), GetUserRole());

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

        // PUT: api/interview-rounds/{interviewRoundId}
        [HttpPut("interview-rounds/{interviewRoundId:int}")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> UpdateInterviewRound(
            int interviewRoundId,
            [FromBody] InterviewRoundUpdateDto dto)
        {
            try
            {
                var result =
                    await _interviewRoundService.UpdateInterviewRoundAsync(
                        interviewRoundId, dto, GetUserId());

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
