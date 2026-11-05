
using CampusPlacementManagementAPI.DTOs.Company;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/companies")]
    [ApiController]
    public class CompaniesController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompaniesController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterCompany(
            CompanyCreateDto dto)
        {
            try
            {
                var result = await _companyService.RegisterCompanyAsync(dto);

                return CreatedAtAction(
                    nameof(GetCompanyById),
                    new { id = result.CompanyId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllCompanies()
        {
            return Ok(await _companyService.GetAllCompaniesAsync());
        }

        [HttpGet("pending")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetPendingCompanies()
        {
            return Ok(await _companyService.GetPendingCompaniesAsync());
        }

        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetCompanyById(int id)
        {
            var result = await _companyService.GetCompanyByIdAsync(id);

            if (result == null)
            {
                return NotFound(new { message = "Company not found." });
            }

            return Ok(result);
        }

        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> UpdateCompany(
            int id, CompanyUpdateDto dto)
        {
            try
            {
                return Ok(await _companyService.UpdateCompanyAsync(id, dto));
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

        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> ApproveCompany(int id)
        {
            try
            {
                await _companyService.UpdateCompanyStatusAsync(
                    id, CompanyStatus.Approved);

                return Ok(new { message = "Company approved successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> RejectCompany(int id)
        {
            try
            {
                await _companyService.UpdateCompanyStatusAsync(
                    id, CompanyStatus.Rejected);

                return Ok(new { message = "Company rejected successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }

        [HttpPut("{id:int}/suspend")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> SuspendCompany(int id)
        {
            try
            {
                await _companyService.UpdateCompanyStatusAsync(
                    id, CompanyStatus.Suspended);

                return Ok(new { message = "Company suspended successfully." });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
        }
    }
}
