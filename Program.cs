
using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services;
using CampusPlacementManagementAPI.Services.Interfaces;

using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using System.Security.Claims;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Database Configuration
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// Identity Configuration
builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
    options.Password.RequiredLength = 8;
    options.Password.RequireDigit = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = true;

    options.User.RequireUniqueEmail = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// JWT Authentication Configuration
var jwtSettings = builder.Configuration.GetSection("Jwt");

var key = Encoding.UTF8.GetBytes(
    jwtSettings["Key"]!);

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultChallengeScheme =
        JwtBearerDefaults.AuthenticationScheme;

    options.DefaultScheme =
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

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(key),

        NameClaimType = ClaimTypes.NameIdentifier,
        RoleClaimType = ClaimTypes.Role,

        ClockSkew = TimeSpan.Zero
    };
});

// Authorization Configuration
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("SuperAdminOnly", policy =>
        policy.RequireRole("SuperAdmin"));

    options.AddPolicy("PlacementOfficerOnly", policy =>
        policy.RequireRole("PlacementOfficer"));

    options.AddPolicy("CompanyHROnly", policy =>
        policy.RequireRole("CompanyHR"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));
});

// College Repository and Service
builder.Services.AddScoped<ICollegeRepository, CollegeRepository>();
builder.Services.AddScoped<ICollegeService, CollegeService>();

// Student Repository and Service
builder.Services.AddScoped<IStudentRepository, StudentRepository>();
builder.Services.AddScoped<IStudentService, StudentService>();

// Placement Officer Repository and Service
builder.Services.AddScoped<IPlacementOfficerRepository, PlacementOfficerRepository>();
builder.Services.AddScoped<IPlacementOfficerService, PlacementOfficerService>();

// Company Repository and Service
builder.Services.AddScoped<ICompanyRepository, CompanyRepository>();
builder.Services.AddScoped<ICompanyService, CompanyService>();

// Authentication Service
builder.Services.AddScoped<IAuthService, AuthService>();

// Controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// SuperAdmin and Role Seeding
if (app.Environment.IsDevelopment())
{
    using (var scope = app.Services.CreateScope())
    {
        var roleManager = scope.ServiceProvider
            .GetRequiredService<RoleManager<IdentityRole>>();

        var userManager = scope.ServiceProvider
            .GetRequiredService<UserManager<ApplicationUser>>();

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

        var adminName =
            builder.Configuration["SeedAdmin:FullName"];

        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            !string.IsNullOrWhiteSpace(adminPassword))
        {
            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    FullName = adminName ?? "System Administrator",
                    AccountStatus = CampusPlacementManagementAPI.Enums.AccountStatus.Approved,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin, adminPassword);

                if (!result.Succeeded)
                {
                    throw new Exception(
                        "SuperAdmin creation failed: " +
                        string.Join(", ",
                            result.Errors.Select(e => e.Description)));
                }
            }

            admin.AccountStatus =
                CampusPlacementManagementAPI.Enums.AccountStatus.Approved;

            await userManager.UpdateAsync(admin);

            if (!await userManager.IsInRoleAsync(admin, "SuperAdmin"))
            {
                await userManager.AddToRoleAsync(admin, "SuperAdmin");
            }
        }
    }

    app.MapOpenApi();
}

// HTTP Pipeline
app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
