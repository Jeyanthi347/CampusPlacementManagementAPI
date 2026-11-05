using System.Security.Claims;
using CampusPlacementManagementAPI.DTOs.CompanyHR;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/company-hrs")]
    [ApiController]
    public class CompanyHRsController : ControllerBase
    {
        private readonly ICompanyHRService _companyHRService;

        public CompanyHRsController(ICompanyHRService companyHRService)
        {
            _companyHRService = companyHRService;
        }

        // Company HR registration
        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCompanyHR(
            CompanyHRRegisterDto dto)
        {
            try
            {
                var result = await _companyHRService.RegisterCompanyHRAsync(dto);

                return StatusCode(201, new
                {
                    message = "Company HR registration submitted successfully. Awaiting SuperAdmin approval.",
                    data = result
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Get all Company HR registrations
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllCompanyHRs()
        {
            var result = await _companyHRService.GetAllCompanyHRsAsync();

            return Ok(result);
        }

        // Get pending Company HR registrations
        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPendingCompanyHRs()
        {
            var result = await _companyHRService.GetPendingCompanyHRsAsync();

            return Ok(result);
        }

        // Get Company HR by ID
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetCompanyHRById(int id)
        {
            var result = await _companyHRService.GetCompanyHRByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Company HR not found." });
            }

            return Ok(result);
        }

        // Approve Company HR
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ApproveCompanyHR(int id)
        {
            try
            {
                var adminUserId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(adminUserId))
                {
                    return Unauthorized();
                }

                var result = await _companyHRService.UpdateCompanyHRStatusAsync(
                    id, adminUserId, true);

                if (!result)
                {
                    return NotFound(new { message = "Company HR not found." });
                }

                return Ok(new
                {
                    message = "Company HR approved successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // Reject Company HR
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RejectCompanyHR(int id)
        {
            try
            {
                var adminUserId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(adminUserId))
                {
                    return Unauthorized();
                }

                var result = await _companyHRService.UpdateCompanyHRStatusAsync(
                    id, adminUserId, false);

                if (!result)
                {
                    return NotFound(new { message = "Company HR not found." });
                }

                return Ok(new
                {
                    message = "Company HR rejected successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}