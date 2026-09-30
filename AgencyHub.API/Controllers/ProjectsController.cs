using AgencyHub.Application.DTOs.Projects;
using AgencyHub.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AgencyHub.API.Controllers;

[ApiController]
[Route("api/projects")]
[Authorize]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Retrieves a paginated, filtered, searched, and sorted list of projects for the current tenant.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProjects([FromQuery] ProjectQueryRequest request)
    {
        var result = await _projectService.GetProjectsAsync(request);
        return Ok(result);
    }

    /// <summary>
    /// Retrieves a specific project by its unique identifier.
    /// </summary>
    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetProject(Guid id)
    {
        var project = await _projectService.GetProjectByIdAsync(id);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }
        return Ok(project);
    }

    /// <summary>
    /// Creates a new project for the current tenant (Restricted to AgencyAdmin).
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
    {
        var project = await _projectService.CreateProjectAsync(request);
        return CreatedAtAction(nameof(GetProject), new { id = project.Id }, project);
    }

    /// <summary>
    /// Updates an existing project (Restricted to AgencyAdmin).
    /// </summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
    {
        var project = await _projectService.UpdateProjectAsync(id, request);
        if (project == null)
        {
            return NotFound(new { message = "Project not found." });
        }
        return Ok(project);
    }

    /// <summary>
    /// Deletes a project by its unique identifier (Restricted to AgencyAdmin).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "AgencyAdmin")]
    public async Task<IActionResult> DeleteProject(Guid id)
    {
        var deleted = await _projectService.DeleteProjectAsync(id);
        if (!deleted)
        {
            return NotFound(new { message = "Project not found." });
        }
        return NoContent();
    }
}