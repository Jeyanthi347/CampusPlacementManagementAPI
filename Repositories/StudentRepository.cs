
using Microsoft.EntityFrameworkCore;
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories.Interfaces;

namespace CampusPlacementManagementAPI.Repositories
{
    public class StudentRepository : IStudentRepository
    {
        private readonly ApplicationDbContext _context;

        public StudentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        // Get student by StudentId
        public async Task<Student?> GetByIdAsync(int studentId)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.StudentId == studentId);
        }

        // Get student by Identity UserId
        public async Task<Student?> GetByUserIdAsync(string userId)
        {
            return await _context.Students
                .Include(s => s.User)
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        // Get student by RegisterNumber within a college
        public async Task<Student?> GetByRegisterNumberAsync(
            int collegeId,
            string registerNumber)
        {
            return await _context.Students
                .FirstOrDefaultAsync(s =>
                    s.CollegeId == collegeId &&
                    s.RegisterNumber == registerNumber);
        }

        // Get all students
        public async Task<List<Student>> GetAllAsync()
        {
            return await _context.Students
                .Include(s => s.User)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get students belonging to a particular college
        public async Task<List<Student>> GetByCollegeIdAsync(int collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Where(s => s.CollegeId == collegeId)
                .AsNoTracking()
                .ToListAsync();
        }

        // Get pending students belonging to a particular college
        public async Task<List<Student>> GetPendingStudentsAsync(int collegeId)
        {
            return await _context.Students
                .Include(s => s.User)
                .Where(s =>
                    s.CollegeId == collegeId &&
                    s.Status == StudentStatus.Pending)
                .AsNoTracking()
                .ToListAsync();
        }

        // Add a new student
        public async Task AddAsync(Student student)
        {
            await _context.Students.AddAsync(student);
            await _context.SaveChangesAsync();
        }

        // Update student details
        public async Task UpdateAsync(Student student)
        {
            _context.Students.Update(student);
            await _context.SaveChangesAsync();
        }

        // Check duplicate register number within a college
        public async Task<bool> ExistsByRegisterNumberAsync(
            int collegeId,
            string registerNumber)
        {
            return await _context.Students.AnyAsync(s =>
                s.CollegeId == collegeId &&
                s.RegisterNumber == registerNumber);
        }
    }
}
