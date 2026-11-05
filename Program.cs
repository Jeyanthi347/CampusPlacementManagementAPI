
using System.Security.Claims;
using System.Text;

using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

using CampusPlacementManagementAPI.Data;
using CampusPlacementManagementAPI.Models;
using CampusPlacementManagementAPI.Repositories;
using CampusPlacementManagementAPI.Repositories.Interfaces;
using CampusPlacementManagementAPI.Services;
using CampusPlacementManagementAPI.Services.Interfaces;

var builder = WebApplication.CreateBuilder(args);

// 1. Database connection
var connectionString = builder.Configuration
    .GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));

// 2. ASP.NET Core Identity
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

// 3. JWT Authentication
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

        RoleClaimType = ClaimTypes.Role,
        NameClaimType = ClaimTypes.NameIdentifier,

        ClockSkew = TimeSpan.Zero
    };
});

// 4. Authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("PlacementOfficerOnly", policy =>
        policy.RequireRole("PlacementOfficer"));

    options.AddPolicy("StudentOnly", policy =>
        policy.RequireRole("Student"));

    options.AddPolicy("StudentManagement", policy =>
        policy.RequireRole("SuperAdmin", "PlacementOfficer"));

    options.AddPolicy("StudentAccess", policy =>
        policy.RequireRole(
            "SuperAdmin",
            "PlacementOfficer",
            "Student"));
});

// 5. Student Repository registration
builder.Services.AddScoped<IStudentRepository, StudentRepository>();

// 6. Student Service registration
builder.Services.AddScoped<IStudentService, StudentService>();

// 7. Controllers and OpenAPI
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// 8. HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.Run();
