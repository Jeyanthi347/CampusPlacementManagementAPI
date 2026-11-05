using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace CampusPlacementManagementAPI.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationDbContext(
            DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<College> Colleges { get; set; }
        public DbSet<Student> Students { get; set; }
        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }
        public DbSet<Company> Companies { get; set; }
        public DbSet<CompanyHR> CompanyHRs { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // College configuration
            builder.Entity<College>(entity =>
            {
                entity.HasKey(x => x.CollegeId);

                entity.Property(x => x.CollegeName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.CollegeCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.UniversityName)
                    .HasMaxLength(200);

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.OfficialEmail)
                    .HasColumnName("Email")
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Website)
                    .HasMaxLength(255);

                entity.Property(x => x.Status)
                    .HasDefaultValue(CollegeStatus.Pending);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => x.CollegeCode)
                    .IsUnique();

                entity.HasIndex(x => x.OfficialEmail)
                    .IsUnique();
            });

            // Student configuration
            builder.Entity<Student>(entity =>
            {
                entity.HasKey(x => x.StudentId);

                entity.HasOne<ApplicationUser>()
                    .WithOne(x => x.Student)
                    .HasForeignKey<Student>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<College>()
                    .WithMany()
                    .HasForeignKey(x => x.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne<PlacementOfficer>()
                    .WithMany()
                    .HasForeignKey(x => x.PlacementOfficerId)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Placement Officer configuration
            builder.Entity<PlacementOfficer>(entity =>
            {
                entity.HasKey(x => x.PlacementOfficerId);

                entity.Property(x => x.Designation)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Status)
                    .HasDefaultValue(PlacementOfficerStatus.Pending);

                entity.HasOne(x => x.User)
                    .WithOne(x => x.PlacementOfficer)
                    .HasForeignKey<PlacementOfficer>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.College)
                    .WithMany()
                    .HasForeignKey(x => x.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);
            });

            // Company configuration
            builder.Entity<Company>(entity =>
            {
                entity.HasKey(x => x.CompanyId);

                entity.Property(x => x.CompanyName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.CompanyEmail)
                    .IsRequired()
                    .HasMaxLength(255);

                entity.Property(x => x.Website)
                    .HasMaxLength(255);

                entity.Property(x => x.Industry)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.Description);

                entity.Property(x => x.Status)
                    .HasDefaultValue(CompanyStatus.Pending);

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasIndex(x => x.CompanyEmail)
                    .IsUnique();
            });

            // Company HR configuration
            builder.Entity<CompanyHR>(entity =>
            {
                entity.HasKey(x => x.CompanyHRId);

                entity.Property(x => x.UserId)
                    .IsRequired();

                entity.Property(x => x.Designation)
                    .HasMaxLength(100);

                // One ApplicationUser belongs to one CompanyHR profile
                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<CompanyHR>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                // One company can have multiple HR users
                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                // SuperAdmin who approved the HR registration
                entity.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });
        }
    }
}