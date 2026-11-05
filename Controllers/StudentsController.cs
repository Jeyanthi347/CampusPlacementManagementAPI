
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

using CampusPlacementManagementAPI.DTOs.Student;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;

namespace CampusPlacementManagementAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class StudentsController : ControllerBase
    {
        private readonly IStudentService _studentService;
        private readonly IStudentRepository _studentRepository;

        public StudentsController(
            IStudentService studentService,
            IStudentRepository studentRepository)
        {
            _studentService = studentService;
            _studentRepository = studentRepository;
        }

        // POST: api/students/enroll
        [HttpPost("enroll")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> EnrollStudent(
            StudentEnrollDto dto)
        {
            var officerUserId = GetCurrentUserId();
            var collegeId = GetCollegeId();
            var placementOfficerId = GetPlacementOfficerId();

            if (officerUserId == null ||
                collegeId == null ||
                placementOfficerId == null)
            {
                return Forbid();
            }

            var result = await _studentService.EnrollStudentAsync(
                dto,
                officerUserId,
                collegeId.Value,
                placementOfficerId.Value);

            return Ok(result);
        }

        // GET: api/students
        [HttpGet]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> GetAllStudents()
        {
            var students = await _studentService.GetAllStudentsAsync();

            return Ok(students);
        }

        // GET: api/students/college
        [HttpGet("college")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> GetStudentsByCollege()
        {
            var collegeId = GetCollegeId();

            if (collegeId == null)
                return Forbid();

            var students =
                await _studentService.GetStudentsByCollegeAsync(
                    collegeId.Value);

            return Ok(students);
        }

        // GET: api/students/pending
        [HttpGet("pending")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> GetPendingStudents()
        {
            var collegeId = GetCollegeId();

            if (collegeId == null)
                return Forbid();

            var students =
                await _studentService.GetPendingStudentsAsync(
                    collegeId.Value);

            return Ok(students);
        }

        // GET: api/students/me
        [HttpGet("me")]
        [Authorize(Roles = "Student")]
        public async Task<IActionResult> GetMyProfile()
        {
            var userId = GetCurrentUserId();

            if (userId == null)
                return Unauthorized();

            var student = await _studentRepository.GetByUserIdAsync(userId);

            if (student == null)
                return NotFound("Student profile not found.");

            var result = await _studentService.GetStudentByIdAsync(
                student.StudentId,
                userId,
                "Student",
                null);

            return Ok(result);
        }

        // GET: api/students/{id}
        [HttpGet("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
        public async Task<IActionResult> GetStudentById(int id)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var collegeId = GetCollegeId();

            if (userId == null || role == null)
                return Unauthorized();

            var student = await _studentService.GetStudentByIdAsync(
                id,
                userId,
                role,
                collegeId);

            if (student == null)
                return NotFound("Student not found.");

            return Ok(student);
        }

        // PUT: api/students/{id}/approve
        [HttpPut("{id:int}/approve")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> ApproveStudent(int id)
        {
            var collegeId = GetCollegeId();

            if (collegeId == null)
                return Forbid();

            await _studentService.ApproveStudentAsync(
                id,
                collegeId.Value);

            return Ok(new
            {
                message = "Student approved successfully."
            });
        }

        // PUT: api/students/{id}/reject
        [HttpPut("{id:int}/reject")]
        [Authorize(Roles = "PlacementOfficer")]
        public async Task<IActionResult> RejectStudent(int id)
        {
            var collegeId = GetCollegeId();

            if (collegeId == null)
                return Forbid();

            await _studentService.RejectStudentAsync(
                id,
                collegeId.Value);

            return Ok(new
            {
                message = "Student rejected successfully."
            });
        }

        // PUT: api/students/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "SuperAdmin,PlacementOfficer,Student")]
        public async Task<IActionResult> UpdateStudent(
            int id,
            StudentUpdateDto dto)
        {
            var userId = GetCurrentUserId();
            var role = GetCurrentUserRole();
            var collegeId = GetCollegeId();

            if (userId == null || role == null)
                return Unauthorized();

            var result = await _studentService.UpdateStudentAsync(
                id,
                dto,
                userId,
                role,
                collegeId);

            return Ok(result);
        }

        // Helper: Get authenticated user's Identity ID
        private string? GetCurrentUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier);
        }

        // Helper: Get authenticated user's role
        private string? GetCurrentUserRole()
        {
            if (User.IsInRole("SuperAdmin"))
                return "SuperAdmin";

            if (User.IsInRole("PlacementOfficer"))
                return "PlacementOfficer";

            if (User.IsInRole("Student"))
                return "Student";

            return null;
        }

        // Helper: Get college ID from JWT claims
        private int? GetCollegeId()
        {
            var value = User.FindFirst("CollegeId")?.Value;

            return int.TryParse(value, out var id) ? id : null;
        }

        // Helper: Get placement officer ID from JWT claims
        private int? GetPlacementOfficerId()
        {
            var value = User.FindFirst("PlacementOfficerId")?.Value;

            return int.TryParse(value, out var id) ? id : null;
        }
    }
}
