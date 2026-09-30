using AgencyHub.Application.DTOs.Common;
using AgencyHub.Application.DTOs.Tasks;
using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Interfaces;

public interface IProjectTaskService
{
    Task<PagedResponse<ProjectTask>> GetTasksAsync(ProjectTaskQueryRequest request);

    Task<ProjectTask?> GetTaskByIdAsync(Guid id);

    Task<ProjectTask> CreateTaskAsync(CreateProjectTaskRequest request);

    Task<ProjectTask?> UpdateTaskAsync(Guid id, UpdateProjectTaskRequest request);

    Task<bool> DeleteTaskAsync(Guid id);
}