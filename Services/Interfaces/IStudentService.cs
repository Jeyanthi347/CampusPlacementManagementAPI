
using CampusPlacementManagementAPI.DTOs.Student;

namespace CampusPlacementManagementAPI.Services.Interfaces
{
    public interface IStudentService
    {
        Task<StudentResponseDto> EnrollStudentAsync(
            StudentEnrollDto dto,
            string placementOfficerUserId,
            int collegeId,
            int placementOfficerId);

        Task<List<StudentResponseDto>> GetAllStudentsAsync();

        Task<List<StudentResponseDto>> GetStudentsByCollegeAsync(
            int collegeId);

        Task<List<StudentResponseDto>> GetPendingStudentsAsync(
            int collegeId);

        Task<StudentResponseDto?> GetStudentByIdAsync(
            int studentId,
            string currentUserId,
            string currentUserRole,
            int? collegeId);

        Task<StudentResponseDto> UpdateStudentAsync(
            int studentId,
            StudentUpdateDto dto,
            string currentUserId,
            string currentUserRole,
            int? collegeId);

        Task<bool> ApproveStudentAsync(
            int studentId,
            int collegeId);

        Task<bool> RejectStudentAsync(
            int studentId,
            int collegeId);
    }
}
