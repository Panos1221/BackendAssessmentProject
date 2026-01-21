using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendProject.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DepartmentsController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentsController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    /// <summary>
    /// Get all departments with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all departments", Description = "Returns a paginated list of all departments")]
    [SwaggerResponse(200, "Returns the list of departments", typeof(PaginatedResult<DepartmentResponse>))]
    public async Task<ActionResult<PaginatedResult<DepartmentResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _departmentService.GetAllAsync(pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get department by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get department by ID", Description = "Returns department information")]
    [SwaggerResponse(200, "Returns the department", typeof(DepartmentResponse))]
    [SwaggerResponse(404, "Department not found")]
    public async Task<ActionResult<DepartmentResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _departmentService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Search departments
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search departments", Description = "Search departments by name or description")]
    [SwaggerResponse(200, "Returns matching departments", typeof(PaginatedResult<DepartmentResponse>))]
    public async Task<ActionResult<PaginatedResult<DepartmentResponse>>> Search(
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term is required");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _departmentService.SearchAsync(q, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get employees in a department
    /// </summary>
    [HttpGet("{id:guid}/employees")]
    [SwaggerOperation(Summary = "Get department employees", Description = "Returns all employees in a department")]
    [SwaggerResponse(200, "Returns the list of employees", typeof(PaginatedResult<EmployeeResponse>))]
    [SwaggerResponse(404, "Department not found")]
    public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetEmployees(
        Guid id,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _departmentService.GetEmployeesAsync(id, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new department
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create department", Description = "Creates a new department")]
    [SwaggerResponse(201, "Department created successfully", typeof(DepartmentResponse))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<DepartmentResponse>> Create(
        [FromBody] CreateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        var department = await _departmentService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = department.Id }, department);
    }

    /// <summary>
    /// Update an existing department
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update department", Description = "Updates an existing department")]
    [SwaggerResponse(200, "Department updated successfully", typeof(DepartmentResponse))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(404, "Department not found")]
    public async Task<ActionResult<DepartmentResponse>> Update(
        Guid id,
        [FromBody] UpdateDepartmentRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _departmentService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Delete a department (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete department", Description = "Soft deletes a department")]
    [SwaggerResponse(204, "Department deleted successfully")]
    [SwaggerResponse(404, "Department not found")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _departmentService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
