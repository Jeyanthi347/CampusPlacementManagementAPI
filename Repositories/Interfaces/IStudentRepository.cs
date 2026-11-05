
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Repositories.Interfaces
{
    public interface IStudentRepository
    {
        Task<Student?> GetByIdAsync(int studentId);

        Task<Student?> GetByUserIdAsync(string userId);

        Task<Student?> GetByRegisterNumberAsync(
            int collegeId,
            string registerNumber);

        Task<List<Student>> GetAllAsync();

        Task<List<Student>> GetByCollegeIdAsync(int collegeId);

        Task<List<Student>> GetPendingStudentsAsync(int collegeId);

        Task AddAsync(Student student);

        Task UpdateAsync(Student student);

        Task<bool> ExistsByRegisterNumberAsync(
            int collegeId,
            string registerNumber);
    }
}
