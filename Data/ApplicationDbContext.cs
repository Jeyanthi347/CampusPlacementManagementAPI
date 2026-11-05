
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

using CampusPlacementManagementAPI.Models;

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

        // Student module DbSet
        public DbSet<Student> Students { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Student table configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(s => s.StudentId);

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
                    .HasColumnType("decimal(4,2)");

                entity.Property(s => s.ResumeUrl)
                    .HasMaxLength(500);

                // Register number must be unique within a college
                entity.HasIndex(s => new
                {
                    s.CollegeId,
                    s.RegisterNumber
                }).IsUnique();

                // One Identity user can have only one student profile
                entity.HasIndex(s => s.UserId)
                    .IsUnique();

                // Student to Identity user relationship
                entity.HasOne(s => s.User)
                    .WithOne()
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
