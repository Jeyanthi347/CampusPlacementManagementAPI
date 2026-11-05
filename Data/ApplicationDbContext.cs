
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Data
{
    public class ApplicationDbContext
        : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; } = null!;

        public DbSet<College> Colleges { get; set; } = null!;

        public DbSet<PlacementOfficer> PlacementOfficers { get; set; } = null!;

        public DbSet<Company> Companies { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // College Configuration
            builder.Entity<College>(entity =>
            {
                entity.HasKey(c => c.CollegeId);

                entity.Property(c => c.CollegeName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.CollegeCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(c => c.UniversityName)
                    .HasMaxLength(200);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(c => c.OfficialEmail)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(c => c.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(c => c.Website)
                    .HasMaxLength(255);

                entity.Property(c => c.Status)
                    .HasDefaultValue(CollegeStatus.Pending);

                entity.HasIndex(c => c.CollegeCode)
                    .IsUnique();

                entity.HasIndex(c => c.OfficialEmail)
                    .IsUnique();
            });

            // Student Configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentId);

                entity.Property(s => s.UserId)
                    .IsRequired();

                entity.Property(s => s.RegisterNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(s => s.Department)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.Degree)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(s => s.CGPA)
                    .HasPrecision(4, 2);

                entity.Property(s => s.ResumeUrl)
                    .HasMaxLength(500);

                entity.Property(s => s.Status)
                    .HasDefaultValue(StudentStatus.Pending);

                entity.HasIndex(s => s.UserId)
                    .IsUnique();

                entity.HasIndex(s => new
                {
                    s.CollegeId,
                    s.RegisterNumber
                }).IsUnique();

                entity.HasOne(s => s.User)
                    .WithOne(u => u.Student)
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<College>()
                    .WithMany()
                    .HasForeignKey(s => s.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Placement Officer Configuration
            builder.Entity<PlacementOfficer>(entity =>
            {
                entity.HasKey(p => p.PlacementOfficerId);

                entity.Property(p => p.UserId)
                    .IsRequired();

                entity.Property(p => p.Designation)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(p => p.Status)
                    .HasDefaultValue(PlacementOfficerStatus.Pending);

                entity.Property(p => p.ApprovedBy)
                    .IsRequired(false);

                entity.HasOne(p => p.User)
                    .WithOne(u => u.PlacementOfficer)
                    .HasForeignKey<PlacementOfficer>(p => p.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.College)
                    .WithMany()
                    .HasForeignKey(p => p.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(p => p.Approver)
                    .WithMany()
                    .HasForeignKey(p => p.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(p => p.UserId)
                    .IsUnique();

                entity.HasIndex(p => p.CollegeId);
            });

            // Company Configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasKey(c => c.CompanyId);

                entity.Property(c => c.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(c => c.CompanyEmail)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(c => c.Website)
                    .HasMaxLength(255);

                entity.Property(c => c.Industry)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(c => c.Description)
                    .IsRequired(false);

                entity.Property(c => c.Status)
                    .HasDefaultValue(CompanyStatus.Pending);

                entity.Property(c => c.CreatedAt)
                    .IsRequired();

                entity.HasIndex(c => c.CompanyEmail)
                    .IsUnique();
            });
        }
    }
}
