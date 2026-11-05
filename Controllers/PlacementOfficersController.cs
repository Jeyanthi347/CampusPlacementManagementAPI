
using System.Security.Claims;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/placement-officers")]
    [ApiController]
    public class PlacementOfficersController : ControllerBase
    {
        private readonly IPlacementOfficerService _officerService;

        public PlacementOfficersController(
            IPlacementOfficerService officerService)
        {
            _officerService = officerService;
        }

        // 1. Public registration
        [AllowAnonymous]
        [HttpPost("register")]
        public async Task<IActionResult> Register(
            CampusPlacementManagementAPI.DTOs.PlacementOfficer.PlacementOfficerRegisterDto dto)
        {
            try
            {
                var result = await _officerService.RegisterAsync(dto);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = result.PlacementOfficerId },
                    result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 2. SuperAdmin gets all officers
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _officerService.GetAllAsync();

            return Ok(result);
        }

        // 3. SuperAdmin gets pending officers
        [Authorize(Roles = "SuperAdmin")]
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            var result = await _officerService.GetPendingAsync();

            return Ok(result);
        }

        // 4. Get officer by ID
        [Authorize(Roles = "SuperAdmin,PlacementOfficer")]
        [HttpGet("{id:int}")]
        public async Task<IActionResult> GetById(int id)
        {
            var officer = await _officerService.GetByIdAsync(id);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement Officer not found."
                });
            }

            if (User.IsInRole("PlacementOfficer"))
            {
                var currentUserId = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (officer.UserId != currentUserId)
                {
                    return Forbid();
                }
            }

            return Ok(officer);
        }

        // 5. SuperAdmin approves officer
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/approve")]
        public async Task<IActionResult> Approve(int id)
        {
            try
            {
                var approvedBy = User.FindFirstValue(
                    ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(approvedBy))
                {
                    return Unauthorized();
                }

                var result = await _officerService.ApproveAsync(
                    id, approvedBy);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Placement Officer not found."
                    });
                }

                return Ok(new
                {
                    message = "Placement Officer approved successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 6. SuperAdmin rejects officer
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/reject")]
        public async Task<IActionResult> Reject(int id)
        {
            try
            {
                var result = await _officerService.RejectAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Placement Officer not found."
                    });
                }

                return Ok(new
                {
                    message = "Placement Officer rejected successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 7. SuperAdmin suspends officer
        [Authorize(Roles = "SuperAdmin")]
        [HttpPut("{id:int}/suspend")]
        public async Task<IActionResult> Suspend(int id)
        {
            try
            {
                var result = await _officerService.SuspendAsync(id);

                if (!result)
                {
                    return NotFound(new
                    {
                        message = "Placement Officer not found."
                    });
                }

                return Ok(new
                {
                    message = "Placement Officer suspended successfully."
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // 8. Officer views own profile
        [Authorize(Roles = "PlacementOfficer")]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var officer = await _officerService.GetByUserIdAsync(userId);

            if (officer == null)
            {
                return NotFound(new
                {
                    message = "Placement Officer profile not found."
                });
            }

            return Ok(officer);
        }
    }
}
