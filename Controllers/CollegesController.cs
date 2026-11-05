
using CampusPlacementManagementAPI.DTOs.College;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CollegesController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegesController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        // College representative submits a registration request.
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> RegisterCollege(CollegeCreateDto dto)
        {
            try
            {
                var result = await _collegeService.CreateCollegeAsync(dto);

                return CreatedAtAction(
                    nameof(GetCollegeById),
                    new { id = result.CollegeId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // SuperAdmin views all college requests.
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAllColleges()
        {
            var colleges = await _collegeService.GetAllCollegesAsync();

            return Ok(colleges);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetCollegeById(int id)
        {
            var college = await _collegeService.GetCollegeByIdAsync(id);

            if (college == null)
            {
                return NotFound(new { message = "College not found." });
            }

            return Ok(college);
        }

        // SuperAdmin updates college details.
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}")]
        public async Task<IActionResult> UpdateCollege(
            int id,
            CollegeUpdateDto dto)
        {
            try
            {
                var college = await _collegeService.UpdateCollegeAsync(id, dto);

                if (college == null)
                {
                    return NotFound(new { message = "College not found." });
                }

                return Ok(college);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> ApproveCollege(int id)
        {
            var updated = await _collegeService.UpdateCollegeStatusAsync(
                id, CollegeStatus.Approved);

            if (!updated)
            {
                return NotFound(new { message = "College not found." });
            }

            return Ok(new { message = "College approved successfully." });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> RejectCollege(int id)
        {
            var updated = await _collegeService.UpdateCollegeStatusAsync(
                id, CollegeStatus.Rejected);

            if (!updated)
            {
                return NotFound(new { message = "College not found." });
            }

            return Ok(new { message = "College rejected successfully." });
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> SuspendCollege(int id)
        {
            var updated = await _collegeService.UpdateCollegeStatusAsync(
                id, CollegeStatus.Suspended);

            if (!updated)
            {
                return NotFound(new { message = "College not found." });
            }

            return Ok(new { message = "College suspended successfully." });
        }
    }
}
