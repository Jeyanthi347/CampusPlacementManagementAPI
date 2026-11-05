using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.Application;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/applications")]
    [ApiController]
    [Authorize]
    public class ApplicationsController : ControllerBase
    {
        private readonly IJobApplicationService _service;

        public ApplicationsController(IJobApplicationService service)
        {
            _service = service;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException("User ID not found.");
        }

        // Student applies for a job
        [HttpPost("/api/jobs/{jobId:int}/apply")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> ApplyForJob(
            int jobId,
            [FromBody] JobApplicationCreateDto dto)
        {
            try
            {
                dto.JobId = jobId;

                var result = await _service.ApplyForJobAsync(
                    GetUserId(), dto);

                return CreatedAtAction(
                    nameof(GetApplicationById),
                    new { id = result.ApplicationId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Student views their applications
        [HttpGet("my")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var result = await _service.GetMyApplicationsAsync(
                    GetUserId());

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        // View application by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "Student,CompanyHR,PlacementOfficer")]
        public async Task<IActionResult> GetApplicationById(int id)
        {
            try
            {
                var role = User.FindFirstValue(ClaimTypes.Role)
                    ?? string.Empty;

                var result = await _service.GetApplicationByIdAsync(
                    id, GetUserId(), role);

                if (result == null)
                {
                    return NotFound(new { message = "Application not found." });
                }

                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Company HR views applications for a job
        [HttpGet("/api/jobs/{jobId:int}/applications")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> GetApplicationsByJob(int jobId)
        {
            try
            {
                var result = await _service.GetJobApplicationsAsync(
                    jobId, GetUserId());

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Company HR shortlists an application
        [HttpPut("{id:int}/shortlist")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> ShortlistApplication(
            int id,
            [FromBody] ApplicationStatusUpdateDto dto)
        {
            try
            {
                await _service.UpdateApplicationStatusAsync(
                    id, GetUserId(), dto, "Shortlisted");

                return Ok(new
                {
                    message = "Application shortlisted successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Company HR rejects an application
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> RejectApplication(
            int id,
            [FromBody] ApplicationStatusUpdateDto dto)
        {
            try
            {
                await _service.UpdateApplicationStatusAsync(
                    id, GetUserId(), dto, "Rejected");

                return Ok(new
                {
                    message = "Application rejected successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // Student withdraws an application
        [HttpPut("{id:int}/withdraw")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> WithdrawApplication(int id)
        {
            try
            {
                await _service.WithdrawApplicationAsync(
                    id, GetUserId());

                return Ok(new
                {
                    message = "Application withdrawn successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}