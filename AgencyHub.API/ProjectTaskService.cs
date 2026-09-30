using AgencyHub.Application.DTOs.Common;
using AgencyHub.Application.DTOs.Tasks;
using AgencyHub.Application.Interfaces;
using AgencyHub.Domain.Entities;
using AgencyHub.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace AgencyHub.API;

public class ProjectTaskService : IProjectTaskService
{
    private readonly ApplicationDbContext _context;
    private readonly ITenantContext _tenantContext;

    public ProjectTaskService(
        ApplicationDbContext context,
        ITenantContext tenantContext)
    {
        _context = context;
        _tenantContext = tenantContext;
    }

    // ========================================================
    // GET TASKS
    // ========================================================

    public async Task<PagedResponse<ProjectTask>> GetTasksAsync(ProjectTaskQueryRequest request)
    {
        EnsureTenant();

        var tenantId = _tenantContext.TenantId!.Value;

        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 10 : Math.Min(request.PageSize, 100);

        var query = _context.ProjectTasks
            .AsNoTracking()
            .Include(x => x.Project)
            .Where(x => x.TenantId == tenantId);

        // Search
        if (!string.IsNullOrWhiteSpace(request.Search))
        {
            var search = request.Search.Trim();
            query = query.Where(x => x.Title.Contains(search) || x.Description.Contains(search));
        }

        // Status
        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            query = query.Where(x => x.Status == status);
        }

        // Priority
        if (!string.IsNullOrWhiteSpace(request.Priority))
        {
            var priority = request.Priority.Trim();
            query = query.Where(x => x.Priority == priority);
        }

        // Project
        if (request.ProjectId.HasValue)
        {
            query = query.Where(x => x.ProjectId == request.ProjectId.Value);
        }

        // Assigned user
        if (request.AssignedToUserId.HasValue)
        {
            query = query.Where(x => x.AssignedToUserId == request.AssignedToUserId.Value);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        return new PagedResponse<ProjectTask>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount,
            TotalPages = totalPages
        };
    }

    // ========================================================
    // GET BY ID
    // ========================================================

    public async Task<ProjectTask?> GetTaskByIdAsync(Guid id)
    {
        EnsureTenant();

        var tenantId = _tenantContext.TenantId!.Value;

        return await _context.ProjectTasks
            .AsNoTracking()
            .Include(x => x.Project)
            .FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);
    }

    // ========================================================
    // CREATE
    // ========================================================

    public async Task<ProjectTask> CreateTaskAsync(CreateProjectTaskRequest request)
    {
        EnsureTenant();

        var tenantId = _tenantContext.TenantId!.Value;

        var projectExists = await _context.Projects.AnyAsync(x => x.Id == request.ProjectId && x.TenantId == tenantId);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project was not found for the current tenant.");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == request.AssignedToUserId.Value && x.TenantId == tenantId);

            if (!userExists)
            {
                throw new KeyNotFoundException("Assigned user was not found for the current tenant.");
            }
        }

        var task = new ProjectTask
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            ProjectId = request.ProjectId,
            AssignedToUserId = request.AssignedToUserId,
            Title = request.Title.Trim(),
            Description = request.Description.Trim(),
            Status = request.Status.Trim(),
            Priority = request.Priority.Trim(),
            DueDate = request.DueDate,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        await _context.ProjectTasks.AddAsync(task);
        await _context.SaveChangesAsync();

        return task;
    }

    // ========================================================
    // UPDATE
    // ========================================================

    public async Task<ProjectTask?> UpdateTaskAsync(Guid id, UpdateProjectTaskRequest request)
    {
        EnsureTenant();

        var tenantId = _tenantContext.TenantId!.Value;

        var task = await _context.ProjectTasks.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (task == null)
        {
            return null;
        }

        var projectExists = await _context.Projects.AnyAsync(x => x.Id == request.ProjectId && x.TenantId == tenantId);

        if (!projectExists)
        {
            throw new KeyNotFoundException("Project was not found for the current tenant.");
        }

        if (request.AssignedToUserId.HasValue)
        {
            var userExists = await _context.Users.AnyAsync(x => x.Id == request.AssignedToUserId.Value && x.TenantId == tenantId);

            if (!userExists)
            {
                throw new KeyNotFoundException("Assigned user was not found for the current tenant.");
            }
        }

        task.ProjectId = request.ProjectId;
        task.AssignedToUserId = request.AssignedToUserId;
        task.Title = request.Title.Trim();
        task.Description = request.Description.Trim();
        task.Status = request.Status.Trim();
        task.Priority = request.Priority.Trim();
        task.DueDate = request.DueDate;
        task.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return task;
    }

    // ========================================================
    // DELETE
    // ========================================================

    public async Task<bool> DeleteTaskAsync(Guid id)
    {
        EnsureTenant();

        var tenantId = _tenantContext.TenantId!.Value;

        var task = await _context.ProjectTasks.FirstOrDefaultAsync(x => x.Id == id && x.TenantId == tenantId);

        if (task == null)
        {
            return false;
        }

        _context.ProjectTasks.Remove(task);
        await _context.SaveChangesAsync();

        return true;
    }

    // ========================================================
    // TENANT SECURITY
    // ========================================================

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