using AgencyHub.Application.DTOs.Common;
using AgencyHub.Application.DTOs.Projects;
using AgencyHub.Domain.Entities;

namespace AgencyHub.Application.Interfaces;

public interface IProjectService
{
    Task<PagedResponse<Project>> GetProjectsAsync(ProjectQueryRequest request);
    Task<Project?> GetProjectByIdAsync(Guid id);
    Task<Project> CreateProjectAsync(CreateProjectRequest request);
    Task<Project?> UpdateProjectAsync(Guid id, UpdateProjectRequest request);
    Task<bool> DeleteProjectAsync(Guid id);
}