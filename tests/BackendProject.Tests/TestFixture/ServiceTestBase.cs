using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using BackendProject.Infrastructure.Data;
using BackendProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendProject.Tests.TestFixture;

/// <summary>
/// Base class for service tests providing common setup for in-memory database and repositories.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly IRepository<Employee> Employees;
    protected readonly IRepository<Department> Departments;
    protected readonly IRepository<Project> Projects;
    protected readonly ISaveChanges SaveChanges;

    protected ServiceTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        Context = new AppDbContext(options);
        Employees = new Repository<Employee>(Context);
        Departments = new Repository<Department>(Context);
        Projects = new Repository<Project>(Context);
        SaveChanges = Context;
    }

    public virtual void Dispose()
    {
        Context.Dispose();
    }
}
