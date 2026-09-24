using AgencyHub.API.Middleware;
using AgencyHub.Application.Services;
using AgencyHub.Infrastructure.Persistence;
using AgencyHub.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

public partial class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add HttpContextAccessor for multi-tenancy resolution
        builder.Services.AddHttpContextAccessor();

        // Register Entity Framework Core with SQL Server
        builder.Services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();
        builder.Services.AddScoped<IClientService, ClientService>();

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHttpsRedirection();

        // Register Custom Tenant Middleware right before authorization/controllers
        app.UseMiddleware<TenantMiddleware>();

        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}