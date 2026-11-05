
using Microsoft.AspNetCore.Identity;
using CampusPlacementManagementAPI.DTOs.Student;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services.Interfaces;


namespace CampusPlacementManagementAPI.Services
{
    public class StudentService : IStudentService
    {
        private readonly IStudentRepository _studentRepository;
        private readonly UserManager<ApplicationUser> _userManager;

        public StudentService(
            IStudentRepository studentRepository,
            UserManager<ApplicationUser> userManager)
        {
            _studentRepository = studentRepository;
            _userManager = userManager;
        }

        public async Task<StudentResponseDto> EnrollStudentAsync(
            StudentEnrollDto dto,
            string placementOfficerUserId,
            int collegeId,
            int placementOfficerId)
        {
            var officer = await _userManager.FindByIdAsync(
                placementOfficerUserId);

            if (officer == null ||
                officer.AccountStatus != AccountStatus.Approved)
            {
                throw new UnauthorizedAccessException(
                    "Only an approved Placement Officer can enroll students.");
            }

            if (await _studentRepository.ExistsByRegisterNumberAsync(
                collegeId, dto.RegisterNumber))
            {
                throw new InvalidOperationException(
                    "Register number already exists in this college.");
            }

            var user = new ApplicationUser
            {
                UserName = dto.Email,
                Email = dto.Email,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AccountStatus = AccountStatus.Pending,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var temporaryPassword = $"Temp@{Guid.NewGuid():N}1a";

            var result = await _userManager.CreateAsync(
                user, temporaryPassword);

            if (!result.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        result.Errors.Select(e => e.Description)));
            }

            var student = new Student
            {
                UserId = user.Id,
                CollegeId = collegeId,
                PlacementOfficerId = placementOfficerId,
                RegisterNumber = dto.RegisterNumber,
                Department = dto.Department,
                Degree = dto.Degree,
                GraduationYear = dto.GraduationYear,
                CGPA = dto.CGPA,
                ActiveBacklogs = dto.ActiveBacklogs,
                ResumeUrl = dto.ResumeUrl,
                Status = StudentStatus.Pending
            };

            try
            {
                await _studentRepository.AddAsync(student);
            }
            catch
            {
                await _userManager.DeleteAsync(user);
                throw;
            }

            return MapToDto(student, user);
        }

        public async Task<List<StudentResponseDto>> GetAllStudentsAsync()
        {
            var students = await _studentRepository.GetAllAsync();

            return students
                .Select(s => MapToDto(s, s.User!))
                .ToList();
        }

        public async Task<List<StudentResponseDto>> GetStudentsByCollegeAsync(
            int collegeId)
        {
            var students = await _studentRepository.GetByCollegeIdAsync(
                collegeId);

            return students
                .Select(s => MapToDto(s, s.User!))
                .ToList();
        }

        public async Task<List<StudentResponseDto>> GetPendingStudentsAsync(
            int collegeId)
        {
            var students = await _studentRepository.GetPendingStudentsAsync(
                collegeId);

            return students
                .Select(s => MapToDto(s, s.User!))
                .ToList();
        }

        public async Task<StudentResponseDto?> GetStudentByIdAsync(
            int studentId,
            string currentUserId,
            string currentUserRole,
            int? collegeId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            if (student == null)
                return null;

            if (currentUserRole == "Student" &&
                student.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You can only view your own profile.");
            }

            if (currentUserRole == "PlacementOfficer" &&
                student.CollegeId != collegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot access students from another college.");
            }

            if (currentUserRole != "Student" &&
                currentUserRole != "PlacementOfficer" &&
                currentUserRole != "SuperAdmin")
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to access this student.");
            }

            return MapToDto(student, student.User!);
        }

        public async Task<StudentResponseDto> UpdateStudentAsync(
            int studentId,
            StudentUpdateDto dto,
            string currentUserId,
            string currentUserRole,
            int? collegeId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found.");

            if (currentUserRole == "Student" &&
                student.UserId != currentUserId)
            {
                throw new UnauthorizedAccessException(
                    "You can only update your own profile.");
            }

            if (currentUserRole == "PlacementOfficer" &&
                student.CollegeId != collegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot update students from another college.");
            }

            if (currentUserRole != "Student" &&
                currentUserRole != "PlacementOfficer" &&
                currentUserRole != "SuperAdmin")
            {
                throw new UnauthorizedAccessException(
                    "You are not allowed to update this student.");
            }

            student.User!.FullName = dto.FullName;
            student.User.PhoneNumber = dto.PhoneNumber;
            student.User.UpdatedAt = DateTime.UtcNow;

            student.Department = dto.Department;
            student.Degree = dto.Degree;
            student.GraduationYear = dto.GraduationYear;
            student.CGPA = dto.CGPA;
            student.ActiveBacklogs = dto.ActiveBacklogs;
            student.ResumeUrl = dto.ResumeUrl;

            var userResult = await _userManager.UpdateAsync(student.User);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        userResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.UpdateAsync(student);

            return MapToDto(student, student.User);
        }

        public async Task<bool> ApproveStudentAsync(
            int studentId,
            int collegeId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found.");

            if (student.CollegeId != collegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot approve students from another college.");
            }

            if (student.Status != StudentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending students can be approved.");
            }

            student.Status = StudentStatus.Approved;
            student.ApprovedAt = DateTime.UtcNow;

            student.User!.AccountStatus = AccountStatus.Approved;
            student.User.UpdatedAt = DateTime.UtcNow;

            var userResult = await _userManager.UpdateAsync(student.User);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        userResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.UpdateAsync(student);

            return true;
        }

        public async Task<bool> RejectStudentAsync(
            int studentId,
            int collegeId)
        {
            var student = await _studentRepository.GetByIdAsync(studentId);

            if (student == null)
                throw new KeyNotFoundException("Student not found.");

            if (student.CollegeId != collegeId)
            {
                throw new UnauthorizedAccessException(
                    "You cannot reject students from another college.");
            }

            if (student.Status != StudentStatus.Pending)
            {
                throw new InvalidOperationException(
                    "Only pending students can be rejected.");
            }

            student.Status = StudentStatus.Rejected;
            student.User!.AccountStatus = AccountStatus.Rejected;
            student.User.UpdatedAt = DateTime.UtcNow;

            var userResult = await _userManager.UpdateAsync(student.User);

            if (!userResult.Succeeded)
            {
                throw new InvalidOperationException(
                    string.Join("; ",
                        userResult.Errors.Select(e => e.Description)));
            }

            await _studentRepository.UpdateAsync(student);

            return true;
        }

        private static StudentResponseDto MapToDto(
            Student student,
            ApplicationUser user)
        {
            return new StudentResponseDto
            {
                StudentId = student.StudentId,
                UserId = student.UserId,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                CollegeId = student.CollegeId,
                PlacementOfficerId = student.PlacementOfficerId,
                RegisterNumber = student.RegisterNumber,
                Department = student.Department,
                Degree = student.Degree,
                GraduationYear = student.GraduationYear,
                CGPA = student.CGPA,
                ActiveBacklogs = student.ActiveBacklogs,
                ResumeUrl = student.ResumeUrl,
                Status = student.Status.ToString(),
                ApprovedAt = student.ApprovedAt
            };
        }
    }
}
