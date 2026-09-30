using AgencyHub.Application.DTOs.Common;
using AgencyHub.Application.DTOs.Projects;
using AgencyHub.Application.Interfaces;
using AgencyHub.Domain.Entities;
using AgencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyHub.API;

public class ProjectService : IProjectService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ProjectService(ApplicationDbContext context, ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    public async Task<PagedResponse<Project>> GetProjectsAsync(ProjectQueryRequest request)
    {
        EnsureTenant();
        var tenantId = _tenantContext.TenantId!.Value;

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = _context.Projects
            .AsNoTracking()
            .Include(x => x.Client)
            .Where(x => x.TenantId == tenantId);

        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x =>
                x.Name.Contains(search) ||
                x.Description.Contains(search) ||
                x.Client.CompanyName.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        if (request.ClientId.HasValue)
        {
            query = query.Where(x => x.ClientId == request.ClientId.Value);
        }

        // ============================================================
        // DYNAMIC SORTING
        // ============================================================
        var sortBy = request.SortBy?.Trim().ToLower();
        var sortOrder = request.SortOrder?.Trim().ToLower();

        query = (sortBy, sortOrder) switch
        {
            ("name", "asc") => query.OrderBy(x => x.Name),
            ("name", "desc") => query.OrderByDescending(x => x.Name),
            ("status", "asc") => query.OrderBy(x => x.Status),
            ("status", "desc") => query.OrderByDescending(x => x.Status),
            ("budget", "asc") => query.OrderBy(x => x.Budget),
            ("budget", "desc") => query.OrderByDescending(x => x.Budget),
            ("createdat", "asc") => query.OrderBy(x => x.CreatedAt),
            _ => query.OrderByDescending(x => x.CreatedAt) // Default sorting: Newest first
        };

        var totalCount = await query.CountAsync();
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<Project>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    public async Task<Project?> GetProjectByIdAsync(Guid id)
    {
        EnsureTenant();
        var tenantId = _tenantContext.TenantId!.Value;

        return await _context.Projects
            .AsNoTracking()
            .Include(x => x.Client)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    public async Task<Project> CreateProjectAsync(CreateProjectRequest request)
    {
        EnsureTenant();
        var tenantId = _tenantContext.TenantId!.Value;

        var clientExists = await _context.Clients.AnyAsync(x => x.Id == request.ClientId && x.TenantId == tenantId);
        if (!clientExists)
        {
            throw new KeyNotFoundException("Client was not found for the current tenant.");
        }

        var project = new Project
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ClientId = request.ClientId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status.Trim(),
            Budget = request.Budget,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.Projects.AddAsync(project);
        await _context.SaveChangesAsync();

        return project;
    }

    public async Task<Project?> UpdateProjectAsync(Guid id, UpdateProjectRequest request)
    {
        EnsureTenant();
        var tenantId = _tenantContext.TenantId!.Value;

        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (project == null) return null;

        var clientExists = await _context.Clients.AnyAsync(x => x.Id == request.ClientId && x.TenantId == tenantId);
        if (!clientExists)
        {
            throw new KeyNotFoundException("Client was not found for the current tenant.");
        }

        project.ClientId = request.ClientId;
        project.Name = request.Name.Trim();
        project.Description = request.Description.Trim();
        project.Status = request.Status.Trim();
        project.Budget = request.Budget;
        project.StartDate = request.StartDate;
        project.EndDate = request.EndDate;
        project.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return project;
    }

    public async Task<bool> DeleteProjectAsync(Guid id)
    {
        EnsureTenant();
        var tenantId = _tenantContext.TenantId!.Value;

        var project = await _context.Projects.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
        if (project == null) return false;

        _context.Projects.Remove(project);
        await _context.SaveChangesAsync();
        return true;
    }

    private void EnsureTenant()
    {
        if (!_tenantContext.IsAuthenticated)
        {
            throw new UnauthorizedAccessException("Authentication is required.");
        }
        if (!_tenantContext.TenantId.HasValue)
        {
            throw new UnauthorizedAccessException("Tenant information is missing.");
        }
    }
}