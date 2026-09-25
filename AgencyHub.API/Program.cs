using AgencyHub.API.Middleware;
using AgencyHub.Application.Services;
using AgencyHub.Infrastructure;
using AgencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add HttpContextAccessor for multi-tenancy resolution
        builder.Services.AddHttpContextAccessor();

        // Register Infrastructure services & EF Core using our clean extension method
        builder.Services.AddInfrastructure(builder.Configuration);

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IClientService, ClientService>();

        // Health Checks & CORS
        builder.Services.AddHealthChecks();
        builder.Services.AddCors(options =>
        {
            options.AddPolicy("Frontend", policy =>
            {
                policy
                    .AllowAnyOrigin()
                    .AllowAnyHeader()
                    .AllowAnyMethod();
            });
        });

        var app = builder.Build();

        // --------------------------------------------------
        // Database initialization & Role Seeding
        // --------------------------------------------------
        using (var scope = app.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            await DbSeeder.SeedAsync(dbContext);
        }

        // --------------------------------------------------
        // HTTP Pipeline
        // --------------------------------------------------
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // CORS must precede routing/endpoints
        app.UseCors("Frontend");

        // Register Custom Tenant Middleware right before authorization/controllers
        app.UseMiddleware<TenantMiddleware>();

        app.UseAuthorization();

        app.MapControllers();
        app.MapHealthChecks("/health");

        app.Run();
    }
}
