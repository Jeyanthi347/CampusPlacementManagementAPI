
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

        // ==========================================
        // DBSETS
        // ==========================================

        public DbSet<College> Colleges { get; set; }

        public DbSet<PlacementOfficer> PlacementOfficers { get; set; }

        public DbSet<Student> Students { get; set; }

        public DbSet<Company> Companies { get; set; }

        public DbSet<CompanyHR> CompanyHRs { get; set; }

        public DbSet<JobOpening> JobOpenings { get; set; }

        public DbSet<JobTargetCollege> JobTargetColleges { get; set; }

        public DbSet<JobApplication> JobApplications { get; set; }

        public DbSet<ApplicationStatusHistory> ApplicationStatusHistories { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ==========================================
            // COLLEGE CONFIGURATION
            // ==========================================

            builder.Entity<College>(entity =>
            {
                entity.HasKey(x => x.CollegeId);

                entity.Property(x => x.CollegeName)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.CollegeCode)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.OfficialEmail)
                    .IsRequired()
                    .HasMaxLength(255)
                    .HasColumnName("Email");

                entity.Property(x => x.PhoneNumber)
                    .IsRequired()
                    .HasMaxLength(20);

                entity.Property(x => x.Address)
                    .IsRequired()
                    .HasMaxLength(500);

                entity.Property(x => x.UniversityName)
                    .HasMaxLength(200);

                entity.Property(x => x.Website)
                    .HasMaxLength(255);

                entity.HasIndex(x => x.CollegeCode)
                    .IsUnique();

                entity.HasIndex(x => x.OfficialEmail)
                    .IsUnique();
            });


            // ==========================================
            // PLACEMENT OFFICER CONFIGURATION
            // ==========================================

            builder.Entity<PlacementOfficer>(entity =>
            {
                entity.HasKey(x => x.PlacementOfficerId);

                entity.Property(x => x.Designation)
                    .IsRequired()
                    .HasMaxLength(100);

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

                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });


            // ==========================================
            // STUDENT CONFIGURATION
            // ==========================================

            builder.Entity<Student>(entity =>
            {
                entity.HasKey(x => x.StudentId);

                entity.Property(x => x.RegisterNumber)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Department)
                    .IsRequired();

                entity.Property(x => x.Degree)
                    .IsRequired();

                entity.Property(x => x.CGPA)
                    .HasPrecision(4, 2);

                entity.HasOne(x => x.User)
                    .WithOne()
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

                entity.HasIndex(x => new
                {
                    x.CollegeId,
                    x.RegisterNumber
                }).IsUnique();

                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });


            // ==========================================
            // COMPANY CONFIGURATION
            // ==========================================

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

                entity.HasIndex(x => x.CompanyEmail)
                    .IsUnique();
            });


            // ==========================================
            // COMPANY HR CONFIGURATION
            // ==========================================

            builder.Entity<CompanyHR>(entity =>
            {
                entity.HasKey(x => x.CompanyHRId);

                entity.Property(x => x.UserId)
                    .IsRequired();

                entity.Property(x => x.Designation)
                    .HasMaxLength(100);

                entity.HasOne(x => x.User)
                    .WithOne()
                    .HasForeignKey<CompanyHR>(x => x.UserId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.Approver)
                    .WithMany()
                    .HasForeignKey(x => x.ApprovedBy)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => x.UserId)
                    .IsUnique();
            });


            // ==========================================
            // JOB OPENING CONFIGURATION
            // ==========================================

            builder.Entity<JobOpening>(entity =>
            {
                entity.HasKey(x => x.JobId);

                entity.Property(x => x.JobTitle)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.JobDescription)
                    .IsRequired();

                entity.Property(x => x.JobType)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(x => x.Location)
                    .IsRequired()
                    .HasMaxLength(200);

                entity.Property(x => x.SalaryPackage)
                    .HasPrecision(12, 2);

                entity.Property(x => x.MinimumCGPA)
                    .HasPrecision(4, 2);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.ApplicationDeadline)
                    .IsRequired();

                entity.Property(x => x.IsOpenToAllColleges)
                    .IsRequired();

                entity.Property(x => x.CreatedAt)
                    .IsRequired();

                entity.HasOne(x => x.Company)
                    .WithMany()
                    .HasForeignKey(x => x.CompanyId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.CreatedByHR)
                    .WithMany()
                    .HasForeignKey(x => x.CreatedByHRId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // ==========================================
            // JOB TARGET COLLEGE CONFIGURATION
            // ==========================================

            builder.Entity<JobTargetCollege>(entity =>
            {
                entity.HasKey(x => x.JobTargetCollegeId);

                entity.HasOne(x => x.Job)
                    .WithMany(x => x.TargetColleges)
                    .HasForeignKey(x => x.JobId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(x => x.College)
                    .WithMany()
                    .HasForeignKey(x => x.CollegeId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasIndex(x => new
                {
                    x.JobId,
                    x.CollegeId
                }).IsUnique();
            });


            // ==========================================
            // JOB APPLICATION CONFIGURATION
            // ==========================================

            builder.Entity<JobApplication>(entity =>
            {
                entity.HasKey(x => x.ApplicationId);

                entity.Property(x => x.Status)
                    .IsRequired();

                entity.Property(x => x.AppliedAt)
                    .IsRequired();

                entity.Property(x => x.LastUpdatedAt)
                    .IsRequired();

                entity.Property(x => x.ResumeUrl)
                    .HasMaxLength(500);

                // One student cannot apply twice for the same job.
                entity.HasIndex(x => new
                {
                    x.StudentId,
                    x.JobId
                }).IsUnique();

                // Job relationship
                entity.HasOne(x => x.Job)
                    .WithMany()
                    .HasForeignKey(x => x.JobId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Student relationship
                entity.HasOne(x => x.Student)
                    .WithMany()
                    .HasForeignKey(x => x.StudentId)
                    .OnDelete(DeleteBehavior.Restrict);
            });


            // ==========================================
            // APPLICATION STATUS HISTORY CONFIGURATION
            // ==========================================

            builder.Entity<ApplicationStatusHistory>(entity =>
            {
                entity.HasKey(x => x.HistoryId);

                entity.Property(x => x.NewStatus)
                    .IsRequired();

                entity.Property(x => x.Remarks)
                    .HasMaxLength(500);

                entity.Property(x => x.UpdatedByUserId)
                    .IsRequired();

                entity.Property(x => x.UpdatedAt)
                    .IsRequired();

                // Application relationship
                entity.HasOne(x => x.Application)
                    .WithMany(x => x.StatusHistory)
                    .HasForeignKey(x => x.ApplicationId)
                    .OnDelete(DeleteBehavior.Restrict);

                // User who changed the application status
                entity.HasOne(x => x.UpdatedByUser)
                    .WithMany()
                    .HasForeignKey(x => x.UpdatedByUserId)
                    .OnDelete(DeleteBehavior.Restrict);
            });
        }
    }
}
