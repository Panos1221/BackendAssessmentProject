using BackendProject.Domain.Interfaces;
using BackendProject.Infrastructure.Data;
using BackendProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Moq;

namespace BackendProject.Tests.Validators;

/// <summary>
/// Base class for validator tests providing common setup for in-memory database and UnitOfWork.
/// </summary>
public abstract class ValidatorTestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly IUnitOfWork UnitOfWork;

    protected ValidatorTestBase()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(warnings => warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        Context = new AppDbContext(options);
        UnitOfWork = new UnitOfWork(Context);
    }

    public virtual void Dispose()
    {
        Context.Dispose();
    }
}
