using BackendProject.Application.Common;
using BackendProject.Application.DTOs;
using BackendProject.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace BackendProject.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class EmployeesController : ControllerBase
{
    private readonly IEmployeeService _employeeService;

    public EmployeesController(IEmployeeService employeeService)
    {
        _employeeService = employeeService;
    }

    /// <summary>
    /// Get all employees with pagination
    /// </summary>
    [HttpGet]
    [SwaggerOperation(Summary = "Get all employees", Description = "Returns a paginated list of all employees")]
    [SwaggerResponse(200, "Returns the list of employees", typeof(PaginatedResult<EmployeeResponse>))]
    public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _employeeService.GetAllAsync(pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Get employee by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [SwaggerOperation(Summary = "Get employee by ID", Description = "Returns detailed employee information including assigned projects")]
    [SwaggerResponse(200, "Returns the employee", typeof(EmployeeDetailResponse))]
    [SwaggerResponse(404, "Employee not found")]
    public async Task<ActionResult<EmployeeDetailResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        return Ok(await _employeeService.GetByIdAsync(id, cancellationToken));
    }

    /// <summary>
    /// Search employees
    /// </summary>
    [HttpGet("search")]
    [SwaggerOperation(Summary = "Search employees", Description = "Search employees by name or email")]
    [SwaggerResponse(200, "Returns matching employees", typeof(PaginatedResult<EmployeeResponse>))]
    public async Task<ActionResult<PaginatedResult<EmployeeResponse>>> Search(
        [FromQuery] string q,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest("Search term is required");

        var pagination = new PaginationParams { PageNumber = pageNumber, PageSize = pageSize };
        var result = await _employeeService.SearchAsync(q, pagination, cancellationToken);
        return Ok(result);
    }

    /// <summary>
    /// Create a new employee
    /// </summary>
    [HttpPost]
    [SwaggerOperation(Summary = "Create employee", Description = "Creates a new employee")]
    [SwaggerResponse(201, "Employee created successfully", typeof(EmployeeResponse))]
    [SwaggerResponse(400, "Invalid request")]
    public async Task<ActionResult<EmployeeResponse>> Create(
        [FromBody] CreateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        var employee = await _employeeService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = employee.Id }, employee);
    }

    /// <summary>
    /// Update an existing employee
    /// </summary>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Summary = "Update employee", Description = "Updates an existing employee")]
    [SwaggerResponse(200, "Employee updated successfully", typeof(EmployeeResponse))]
    [SwaggerResponse(400, "Invalid request")]
    [SwaggerResponse(404, "Employee not found")]
    public async Task<ActionResult<EmployeeResponse>> Update(
        Guid id,
        [FromBody] UpdateEmployeeRequest request,
        CancellationToken cancellationToken = default)
    {
        return Ok(await _employeeService.UpdateAsync(id, request, cancellationToken));
    }

    /// <summary>
    /// Delete an employee (soft delete)
    /// </summary>
    [HttpDelete("{id:guid}")]
    [SwaggerOperation(Summary = "Delete employee", Description = "Soft deletes an employee")]
    [SwaggerResponse(204, "Employee deleted successfully")]
    [SwaggerResponse(404, "Employee not found")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        await _employeeService.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    /// <summary>
    /// Assign employee to a project
    /// </summary>
    [HttpPost("{id:guid}/projects/{projectId:guid}")]
    [SwaggerOperation(Summary = "Assign to project", Description = "Assigns an employee to a project")]
    [SwaggerResponse(200, "Employee assigned to project")]
    [SwaggerResponse(404, "Employee or project not found")]
    public async Task<IActionResult> AssignToProject(Guid id, Guid projectId, CancellationToken cancellationToken = default)
    {
        await _employeeService.AssignToProjectAsync(id, projectId, cancellationToken);
        return Ok();
    }

    /// <summary>
    /// Remove employee from a project
    /// </summary>
    [HttpDelete("{id:guid}/projects/{projectId:guid}")]
    [SwaggerOperation(Summary = "Remove from project", Description = "Removes an employee from a project")]
    [SwaggerResponse(200, "Employee removed from project")]
    [SwaggerResponse(404, "Employee or assignment not found")]
    public async Task<IActionResult> RemoveFromProject(Guid id, Guid projectId, CancellationToken cancellationToken = default)
    {
        await _employeeService.RemoveFromProjectAsync(id, projectId, cancellationToken);
        return Ok();
    }
}
