using System.Text;
using AgencyHub.Application.Interfaces;
using AgencyHub.API.Services;
using AgencyHub.API.Middleware;
using AgencyHub.API.Options;
using AgencyHub.Application.Services;
using AgencyHub.Application.Validators;
using AgencyHub.Infrastructure;
using AgencyHub.Infrastructure.Persistence;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// ============================================================
// CONTROLLERS & CUSTOM VALIDATION ERROR RESPONSE (STEP 4)
// ============================================================

builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = actionContext =>
        {
            var errors = actionContext.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .ToDictionary(
                    kvp => kvp.Key,
                    kvp => kvp.Value.Errors.Select(e => e.ErrorMessage).ToArray()
                );

            var errorResponse = new
            {
                status = 400,
                title = "Validation Failed",
                errors = errors
            };

            return new Microsoft.AspNetCore.Mvc.BadRequestObjectResult(errorResponse);
        };
    });

// ============================================================
// HTTP CONTEXT ACCESSOR
// ============================================================

builder.Services.AddHttpContextAccessor();

// ============================================================
// TENANT CONTEXT
// ============================================================

builder.Services.AddScoped<ITenantContext, TenantContext>();

// ============================================================
// FLUENT VALIDATION
// ============================================================

builder.Services
    .AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();

// ============================================================
// INFRASTRUCTURE
// ============================================================

builder.Services.AddInfrastructure(builder.Configuration);

// ============================================================
// APPLICATION SERVICES
// ============================================================

builder.Services.AddScoped<IAuthService, AuthenticationService>();

builder.Services.AddScoped<
    IClientService,
    AgencyHub.API.ClientService>();

builder.Services.AddScoped<
    IProjectService,
    AgencyHub.API.ProjectService>();

builder.Services.AddScoped<
    IProjectTaskService,
    AgencyHub.API.ProjectTaskService>();

// ============================================================
// JWT SETTINGS
// ============================================================

builder.Services.Configure<JwtSettings>(
    builder.Configuration.GetSection("JwtSettings"));

// ============================================================
// READ JWT CONFIGURATION
// ============================================================

var jwtSettings = builder.Configuration.GetSection("JwtSettings");

var jwtKey = jwtSettings["Key"];
var jwtIssuer = jwtSettings["Issuer"];
var jwtAudience = jwtSettings["Audience"];

if (string.IsNullOrWhiteSpace(jwtKey))
{
    throw new InvalidOperationException(
        "JWT Key is missing from configuration.");
}

if (string.IsNullOrWhiteSpace(jwtIssuer))
{
    throw new InvalidOperationException(
        "JWT Issuer is missing from configuration.");
}

if (string.IsNullOrWhiteSpace(jwtAudience))
{
    throw new InvalidOperationException(
        "JWT Audience is missing from configuration.");
}

// ============================================================
// AUTHENTICATION
// ============================================================

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters =
            new TokenValidationParameters
            {
                // --------------------------------------------
                // SIGNATURE
                // --------------------------------------------

                ValidateIssuerSigningKey = true,

                IssuerSigningKey =
                    new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtKey)),

                // --------------------------------------------
                // ISSUER
                // --------------------------------------------

                ValidateIssuer = true,

                ValidIssuer = jwtIssuer,

                // --------------------------------------------
                // AUDIENCE
                // --------------------------------------------

                ValidateAudience = true,

                ValidAudience = jwtAudience,

                // --------------------------------------------
                // EXPIRATION
                // --------------------------------------------

                ValidateLifetime = true,

                ClockSkew = TimeSpan.Zero,

                // --------------------------------------------
                // CLAIMS
                // --------------------------------------------

                NameClaimType = "userId",

                RoleClaimType =
                    System.Security.Claims.ClaimTypes.Role
            };

        // ----------------------------------------------------
        // OPTIONAL DEBUGGING
        // ----------------------------------------------------
        // Temporarily useful while fixing JWT problems.
        // Do NOT keep detailed token logging in production.

        options.Events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                Console.WriteLine(
                    $"JWT Authentication Failed: {context.Exception.Message}");

                return Task.CompletedTask;
            }
        };
    });

// ============================================================
// AUTHORIZATION
// ============================================================

builder.Services.AddAuthorization();

// ============================================================
// CORS
// ============================================================

builder.Services.AddCors(options =>
{
    options.AddPolicy(
        "Frontend",
        policy =>
        {
            policy
                .WithOrigins(
                    "http://localhost:5173",
                    "https://localhost:5173")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .AllowCredentials();
        });
});

// ============================================================
// HEALTH CHECKS
// ============================================================

builder.Services
    .AddHealthChecks()
    .AddCheck(
        "self",
        () => HealthCheckResult.Healthy(),
        tags: new[]
        {
            "live"
        })
    .AddDbContextCheck<ApplicationDbContext>(
        "database",
        tags: new[]
        {
            "ready"
        });

// ============================================================
// SWAGGER
// ============================================================

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Title = "AgencyHub API",
            Version = "v1",
            Description = "Multi-tenant agency management API"
        });

    // --------------------------------------------------------
    // JWT SECURITY DEFINITION
    // --------------------------------------------------------

    options.AddSecurityDefinition(
        "Bearer",
        new OpenApiSecurityScheme
        {
            Name = "Authorization",

            Type = SecuritySchemeType.Http,

            Scheme = "bearer",

            BearerFormat = "JWT",

            In = ParameterLocation.Header,

            Description =
                "Enter your JWT token. Example: Bearer {token}"
        });

    // --------------------------------------------------------
    // JWT SECURITY REQUIREMENT
    // --------------------------------------------------------

    options.AddSecurityRequirement(
        document =>
            new OpenApiSecurityRequirement
            {
                [
                    new OpenApiSecuritySchemeReference(
                        "Bearer",
                        document)
                ] = []
            });
});

// ============================================================
// BUILD
// ============================================================

var app = builder.Build();

// ============================================================
// DATABASE SEEDING
// ============================================================

using (var scope = app.Services.CreateScope())
{
    var dbContext =
        scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

    await DbSeeder.SeedAsync(dbContext);
}

// ============================================================
// GLOBAL EXCEPTION HANDLING (PLACED AT TOP OF PIPELINE)
// ============================================================

app.UseMiddleware<GlobalExceptionMiddleware>();

// ============================================================
// SWAGGER
// ============================================================

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();

    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint(
            "/swagger/v1/swagger.json",
            "AgencyHub API v1");

        options.RoutePrefix = "swagger";
    });
}

// ============================================================
// HTTPS
// ============================================================

app.UseHttpsRedirection();

// ============================================================
// CORS
// ============================================================

app.UseCors("Frontend");

// ============================================================
// AUTHENTICATION
// ============================================================

app.UseAuthentication();

// ============================================================
// TENANT MIDDLEWARE
// ============================================================

app.UseMiddleware<TenantMiddleware>();

// ============================================================
// AUTHORIZATION
// ============================================================

app.UseAuthorization();

// ============================================================
// CONTROLLERS
// ============================================================

app.MapControllers();

// ============================================================
// HEALTH CHECKS
// ============================================================

app.MapHealthChecks("/health");

app.MapHealthChecks(
    "/health/live",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("live")
    });

app.MapHealthChecks(
    "/health/ready",
    new HealthCheckOptions
    {
        Predicate = check =>
            check.Tags.Contains("ready")
    });

// ============================================================
// RUN
// ============================================================

app.Run();