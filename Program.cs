
using System.Security.Claims;
using System.Text;
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Enums;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services;
using CampusPlacementManagementAPI.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

// --------------------------------------------------
// DATABASE CONFIGURATION
// --------------------------------------------------

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// --------------------------------------------------
// IDENTITY CONFIGURATION
// --------------------------------------------------

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 8;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// --------------------------------------------------
// JWT AUTHENTICATION
// --------------------------------------------------

var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("JWT Key is missing.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("JWT Issuer is missing.");

var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("JWT Audience is missing.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.MapInboundClaims = false;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,

        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtKey)),

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,

        ClockSkew = TimeSpan.Zero
    };
});

// --------------------------------------------------
// AUTHORIZATION
// --------------------------------------------------

builder.Services.AddAuthorization();

// --------------------------------------------------
// REPOSITORY REGISTRATIONS
// --------------------------------------------------

builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();

builder.Services.AddScoped<IStudentRepository, StudentRepository>();

builder.Services.AddScoped<IPlacementOfficerRepository,
    PlacementOfficerRepository>();

builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();

builder.Services.AddScoped<ICompanyHRRepository, CompanyHRRepository>();

builder.Services.AddScoped<IJobRepository, JobRepository>();

// --------------------------------------------------
// SERVICE REGISTRATIONS
// --------------------------------------------------

builder.Services.AddScoped<IAuthService, AuthService>();

builder.Services.AddScoped<ICollegeService, CollegeService>();

builder.Services.AddScoped<IStudentService, StudentService>();

builder.Services.AddScoped<IPlacementOfficerService,
    PlacementOfficerService>();

builder.Services.AddScoped<ICompanyService, CompanyService>();

builder.Services.AddScoped<ICompanyHRService, CompanyHRService>();

builder.Services.AddScoped<IJobService, JobService>();

// --------------------------------------------------
// CONTROLLERS AND SWAGGER
// --------------------------------------------------

builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen();

var app = builder.Build();

// --------------------------------------------------
// SEED ROLES AND SUPER ADMIN
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var services = scope.ServiceProvider;

        var roleManager =
            services.GetRequiredService<RoleManager<IdentityRole>>();

        var userManager =
            services.GetRequiredService<UserManager<ApplicationUser>>();

        string[] roles =
        {
            "SuperAdmin",
            "PlacementOfficer",
            "CompanyHR",
            "Student"
        };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(
                    new IdentityRole(role));
            }
        }

        var adminEmail =
            builder.Configuration["SeedAdmin:Email"];

        var adminPassword =
            builder.Configuration["SeedAdmin:Password"];

        var adminFullName =
            builder.Configuration["SeedAdmin:FullName"];

        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            var adminUser =
                await userManager.FindByEmailAsync(adminEmail);

            if (adminUser == null)
            {
                adminUser = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminFullName ?? "System Administrator",
                    EmailConfirmed = true,
                    AccountStatus = AccountStatus.Approved,
                    CreatedAt = DateTime.UtcNow
                };

                var result = await userManager.CreateAsync(
                    adminUser, adminPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(
                        ", ",
                        result.Errors.Select(e => e.Description));

                    throw new InvalidOperationException(
                        "SuperAdmin creation failed: " + errors);
                }
            }

            if (!await userManager.IsInRoleAsync(
                adminUser, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(
                    adminUser, "SuperAdmin");
            }
        }
    }
}

// --------------------------------------------------
// HTTP PIPELINE
// --------------------------------------------------

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
