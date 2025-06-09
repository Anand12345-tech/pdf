using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using PdfManagement.API.HealthChecks;
using PdfManagement.API.Models.Common;
using PdfManagement.Core.Application.Interfaces;
using PdfManagement.Core.Application.Services;
using PdfManagement.Core.Domain.Entities;
using PdfManagement.Core.Domain.Interfaces;
using PdfManagement.Infrastructure.Data.Context;
using PdfManagement.Infrastructure.Data.Repositories;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using PdfManagement.Data.Repositories.Implementations;
using PdfManagement.Data.Repositories.Interfaces;
using PdfManagement.Services.Implementations;
using PdfManagement.Services.Interfaces;
using PdfManagement.Infrastructure.Services;

// Load environment variables from .env file
var envFilePath = Path.Combine(Directory.GetCurrentDirectory(), ".env");
if (File.Exists(envFilePath))
{
    foreach (var line in File.ReadAllLines(envFilePath))
    {
        if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
            continue;

        var parts = line.Split('=', 2);
        if (parts.Length == 2)
        {
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            // Don't override existing environment variables
            if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
            {
                Environment.SetEnvironmentVariable(key, value);
            }
        }
    }
    Console.WriteLine("Loaded environment variables from .env file");
}
else
{
    // Try parent directory as fallback
    envFilePath = Path.Combine(Directory.GetCurrentDirectory(), "..", ".env");
    if (File.Exists(envFilePath))
    {
        foreach (var line in File.ReadAllLines(envFilePath))
        {
            if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                continue;

            var parts = line.Split('=', 2);
            if (parts.Length == 2)
            {
                var key = parts[0].Trim();
                var value = parts[1].Trim();

                // Don't override existing environment variables
                if (string.IsNullOrEmpty(Environment.GetEnvironmentVariable(key)))
                {
                    Environment.SetEnvironmentVariable(key, value);
                }
            }
        }
        Console.WriteLine("Loaded environment variables from parent directory .env file");
    }
    else
    {
        Console.WriteLine(".env file not found, using system environment variables");
    }
}

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
// Configure Kestrel to listen on all addresses
builder.WebHost.ConfigureKestrel(serverOptions =>
{
    serverOptions.ListenAnyIP(5000); // Listen on port 5000 for HTTP
    serverOptions.ListenAnyIP(5001, listenOptions => // Listen on port 5001 for HTTPS
    {
        listenOptions.UseHttps();
    });
});

// Add services to the container.
//builder.Services.AddDbContext<ApplicationDbContext>(options =>
//    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Get connection string from environment variable or configuration
var connectionString = Environment.GetEnvironmentVariable("DB_CONNECTION_STRING") ??
                      builder.Configuration.GetConnectionString("DefaultConnection");

// Add logging to debug connection issues
Console.WriteLine($"Using connection string: {MaskConnectionString(connectionString)}");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseNpgsql(connectionString, npgsqlOptions =>
    {
        // Add retry logic for transient errors
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorCodesToAdd: null);
    });
});

// Helper method to safely mask connection string
static string MaskConnectionString(string connectionString)
{
    if (string.IsNullOrEmpty(connectionString)) return string.Empty;

    try
    {
        // Simple approach to mask password
        return Regex.Replace(connectionString,
            @"Password=([^;]*)",
            "Password=***",
            RegexOptions.IgnoreCase);
    }
    catch
    {
        // If anything goes wrong, return a safe version
        return "Connection string present but not displayed for security";
    }
}

// Register repositories
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IPdfDocumentRepository, PdfDocumentRepository>();
builder.Services.AddScoped<IPdfCommentRepository, PdfCommentRepository>();

// Register services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPdfDocumentService, PdfDocumentService>();
builder.Services.AddScoped<IPdfCommentService, PdfCommentService>();
builder.Services.AddScoped<IPublicAccessService, PublicAccessService>();

// Use Google Storage Service as the primary file storage service
builder.Services.AddScoped<IGoogleStorageService, GoogleStorageService>();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowedOrigins",
        policy =>
        {
            var allowedOriginsEnv = Environment.GetEnvironmentVariable("ALLOWED_ORIGINS");
            string[] allowedOrigins;

            if (!string.IsNullOrEmpty(allowedOriginsEnv))
            {
                allowedOrigins = allowedOriginsEnv.Split(',')
                    .Where(o => !string.IsNullOrWhiteSpace(o))
                    .ToArray();
            }
            else
            {
                allowedOrigins = new[] { "http://localhost:3000" };
            }

            if (allowedOrigins.Length > 0)
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
            else
            {
                policy.AllowAnyOrigin()
                      .AllowAnyHeader()
                      .AllowAnyMethod();
            }
        });
});



builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

// Configure JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = Environment.GetEnvironmentVariable("JWT_ISSUER") ?? builder.Configuration["Jwt:Issuer"],
        ValidAudience = Environment.GetEnvironmentVariable("JWT_AUDIENCE") ?? builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(
                Environment.GetEnvironmentVariable("JWT_KEY") ??
                builder.Configuration["Jwt:Key"] ??
                string.Empty
            )
        )
    };
});

// Configure health checks
builder.Services.AddHealthChecks()
    .AddCheck<DbContextHealthCheck<ApplicationDbContext>>("database")
    .AddCheck("google-storage", () => {
        try
        {
            // Check if we have environment variables for Google credentials
            var googleClientEmail = Environment.GetEnvironmentVariable("GOOGLE_CLIENT_EMAIL");
            var googlePrivateKey = Environment.GetEnvironmentVariable("GOOGLE_PRIVATE_KEY");
            
            if (!string.IsNullOrEmpty(googleClientEmail) && !string.IsNullOrEmpty(googlePrivateKey))
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Google credentials found in environment variables");
            }
            
            // Fall back to checking for credentials file
            var googleCredentialsPath = Environment.GetEnvironmentVariable("GOOGLE_APPLICATION_CREDENTIALS") ??
                                       builder.Configuration["GoogleDrive:ServiceAccountPath"];
            if (string.IsNullOrEmpty(googleCredentialsPath) || !File.Exists(googleCredentialsPath))
            {
                return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy("Google credentials not found in environment variables or file");
            }
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy("Google credentials file found");
        }
        catch (Exception ex)
        {
            return Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Unhealthy(ex.Message);
        }
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "PDF Management API", Version = "v1" });
    c.EnableAnnotations();

    // Add JWT authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
// Always enable Swagger for this application
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "PDF Management API v1");
    // c.RoutePrefix = string.Empty; // Set Swagger UI at the root
});

// Apply CORS before routing
app.UseCors("AllowedOrigins");

// Serve static files from wwwroot
app.UseStaticFiles();

// Configure static files for images
app.UseStaticFiles();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Add health check endpoints
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";

        var response = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            }),
            totalDuration = report.TotalDuration.TotalMilliseconds
        };

        await context.Response.WriteAsync(JsonSerializer.Serialize(response));
    }
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = _ => false, // Minimal check just to see if the app is running
    ResponseWriter = async (context, report) =>
    {
        await context.Response.WriteAsync("Alive");
    }
});

// Simple health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }));

Console.WriteLine("Starting PDF Management API on ports 5000 (HTTP) and 5001 (HTTPS)");

app.Run();
