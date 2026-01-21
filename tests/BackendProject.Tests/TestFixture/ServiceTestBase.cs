using BackendProject.Infrastructure.Data;
using BackendProject.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace BackendProject.Tests.TestFixture;

/// <summary>
/// Base class for service tests providing common setup for in-memory database and UnitOfWork.
/// </summary>
public abstract class ServiceTestBase : IDisposable
{
    protected readonly AppDbContext Context;
    protected readonly UnitOfWork UnitOfWork;

    protected ServiceTestBase()
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
