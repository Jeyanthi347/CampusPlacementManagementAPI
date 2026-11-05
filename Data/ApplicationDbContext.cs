
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using CampusPlacementManagementAPI.Models;

namespace CampusPlacementManagementAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Student> Students { get; set; }

        public DbSet<College> Colleges { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // College Configuration
            builder.Entity<College>(entity =>
            {
                entity.HasKey(c => c.CollegeId);

                entity.Property(c => c.CollegeName)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.CollegeCode)
                    .IsRequired()
                    .HasMaxLength(30);

                entity.Property(c => c.Address)
                    .IsRequired()
                    .HasMaxLength(250);

                entity.Property(c => c.City)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.State)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(c => c.Email)
                    .IsRequired()
                    .HasMaxLength(150);

                entity.Property(c => c.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.HasIndex(c => c.CollegeCode)
                    .IsUnique();

                entity.HasIndex(c => c.Email)
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
                    .HasColumnType("decimal(4,2)");

                entity.Property(s => s.ResumeUrl)
                    .HasMaxLength(500);

                entity.HasIndex(s => new
                {
                    s.CollegeId,
                    s.RegisterNumber
                }).IsUnique();

                entity.HasIndex(s => s.UserId)
                    .IsUnique();

                entity.HasOne(s => s.User)
                    .WithOne()
                    .HasForeignKey<Student>(s => s.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<College>()
                    .WithMany()
                    .HasForeignKey(s => s.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
