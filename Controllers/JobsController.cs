
using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.Job;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    [Authorize]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? throw new UnauthorizedAccessException(
                    "User ID not found in token.");
        }

        // Company HR creates a job
        [HttpPost]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> CreateJob(
            [FromBody] JobCreateDto dto)
        {
            try
            {
                var result = await _jobService.CreateJobAsync(
                    dto, GetUserId());

                return CreatedAtAction(
                    nameof(GetJobById),
                    new { id = result.JobId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Forbid();
            }
        }

        // Get jobs based on logged-in user's role
        [HttpGet]
        [Authorize(Roles = "SuperAdmin,CompanyHR,Student")]
        public async Task<IActionResult> GetAllJobs()
        {
            try
            {
                var userId = GetUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

                var result = await _jobService.GetAllJobsAsync(
                    userId, role);

                return Ok(result);
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

        // Get a job by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,CompanyHR,Student")]
        public async Task<IActionResult> GetJobById(int id)
        {
            try
            {
                var userId = GetUserId();
                var role = User.FindFirstValue(ClaimTypes.Role) ?? "";

                var result = await _jobService.GetJobByIdAsync(
                    id, userId, role);

                if (result == null)
                    return NotFound(new { message = "Job not found." });

                return Ok(result);
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

        // Company HR updates their own draft job
        [HttpPut("{id:int}")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> UpdateJob(
            int id, [FromBody] JobUpdateDto dto)
        {
            try
            {
                var result = await _jobService.UpdateJobAsync(
                    id, dto, GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // Company HR publishes their draft job
        [HttpPut("{id:int}/publish")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> PublishJob(int id)
        {
            try
            {
                var result = await _jobService.PublishJobAsync(
                    id, GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // Company HR closes their published job
        [HttpPut("{id:int}/close")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> CloseJob(int id)
        {
            try
            {
                var result = await _jobService.CloseJobAsync(
                    id, GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // Company HR deletes their own draft job
        [HttpDelete("{id:int}")]
        [Authorize(Roles = "CompanyHR")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                await _jobService.DeleteJobAsync(id, GetUserId());

                return Ok(new
                {
                    message = "Job deleted successfully."
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
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

        // Students view eligible jobs
        [HttpGet("eligible")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetEligibleJobs()
        {
            try
            {
                var result = await _jobService.GetEligibleJobsAsync(
                    GetUserId());

                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Students check eligibility for a particular job
        [HttpGet("{id:int}/eligibility")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> CheckEligibility(int id)
        {
            try
            {
                var result = await _jobService.CheckEligibilityAsync(
                    id, GetUserId());

                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
