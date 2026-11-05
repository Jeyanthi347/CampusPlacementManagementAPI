
using CampusPlacementManagementAPI.DTOs.College;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class CollegesController : ControllerBase
    {
        private readonly ICollegeService _collegeService;

        public CollegesController(ICollegeService collegeService)
        {
            _collegeService = collegeService;
        }

        // GET: api/colleges
        [HttpGet]
        public async Task<IActionResult> GetAllColleges()
        {
            var colleges = await _collegeService.GetAllCollegesAsync();

            return Ok(colleges);
        }

        // GET: api/colleges/1
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollegeById(int id)
        {
            var college = await _collegeService.GetCollegeByIdAsync(id);

            if (college == null)
                return NotFound(new { message = "College not found." });

            return Ok(college);
        }

        // POST: api/colleges
        [HttpPost]
        public async Task<IActionResult> CreateCollege(CollegeCreateDto dto)
        {
            try
            {
                var college = await _collegeService.CreateCollegeAsync(dto);

                return CreatedAtAction(
                    nameof(GetCollegeById),
                    new { id = college.CollegeId },
                    college);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // PUT: api/colleges/1
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollege(
            int id,
            CollegeUpdateDto dto)
        {
            try
            {
                var college = await _collegeService.UpdateCollegeAsync(id, dto);

                if (college == null)
                    return NotFound(new { message = "College not found." });

                return Ok(college);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        // DELETE: api/colleges/1
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollege(int id)
        {
            try
            {
                var result = await _collegeService.DeleteCollegeAsync(id);

                if (!result)
                    return NotFound(new { message = "College not found." });

                return Ok(new { message = "College deleted successfully." });
            }
            catch (DbUpdateException)
            {
                return Conflict(new
                {
                    message = "This college cannot be deleted because it is being used by other records."
                });
            }
        }
    }
}
