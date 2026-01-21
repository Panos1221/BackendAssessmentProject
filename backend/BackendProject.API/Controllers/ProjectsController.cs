using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendProject.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ProjectsController : ControllerBase
{
    private readonly IProjectService _projectService;

    public ProjectsController(IProjectService projectService)
    {
        _projectService = projectService;
    }

    /// <summary>
    /// Get all projects with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all projects", Description = "Returns a paginated list of all projects")]
    [SwaggerResponse(200, "Returns the list of projects", typeof(PaginatedResult<ProjectResponse>))]
    public async Task<ActionResult<PaginatedResult<ProjectResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _projectService.GetAllAsync(pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get project by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get project by ID", Description = "Returns project information")]
    [SwaggerResponse(200, "Returns the project", typeof(ProjectResponse))]
    [SwaggerResponse(404, "Project not found")]
    public async Task<ActionResult<ProjectResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _projectService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Search projects
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search projects", Description = "Search projects by name or description")]
    [SwaggerResponse(200, "Returns matching projects", typeof(PaginatedResult<ProjectResponse>))]
    public async Task<ActionResult<PaginatedResult<ProjectResponse>>> Search(
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term is required");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _projectService.SearchAsync(q, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get employees assigned to a project
    /// </summary>
    [HttpGet("{id:guid}/employees")]
    [SwaggerOperation(Summary = "Get project employees", Description = "Returns all employees assigned to a project")]
    [SwaggerResponse(200, "Returns the list of employees", typeof(PaginatedResult<EmployeeResponse>))]
    [SwaggerResponse(404, "Project not found")]
    public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetEmployees(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _projectService.GetEmployeesAsync(id, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new project
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create project", Description = "Creates a new project")]
    [SwaggerResponse(201, "Project created successfully", typeof(ProjectResponse))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<ProjectResponse>> Create(
        [FromBody] CreateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        var project = await _projectService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = project.Id }, project);
    }

    /// <summary>
    /// Update an existing project
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update project", Description = "Updates an existing project")]
    [SwaggerResponse(200, "Project updated successfully", typeof(ProjectResponse))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(404, "Project not found")]
    public async Task<ActionResult<ProjectResponse>> Update(
        Guid id,
        [FromBody] UpdateProjectRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _projectService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Delete a project (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete project", Description = "Soft deletes a project")]
    [SwaggerResponse(204, "Project deleted successfully")]
    [SwaggerResponse(404, "Project not found")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _projectService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
