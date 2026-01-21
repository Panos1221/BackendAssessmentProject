using BackendProject.Domain.Entities;
using BackendProject.Domain.Interfaces;
using BackendProject.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Storage;

namespace BackendProject.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly AppDbContext _context;
    private IDbContextTransaction? _transaction;
    private IRepository<Employee>? _employees;
    private IRepository<Department>? _departments;
    private IRepository<Project>? _projects;
    private bool _disposed;

    public UnitOfWork(AppDbContext context)
    {
        _context = context;
    }

    public IRepository<Employee> Employees => 
        _employees ??= new Repository<Employee>(_context);

    public IRepository<Department> Departments => 
        _departments ??= new Repository<Department>(_context);

    public IRepository<Project> Projects => 
        _projects ??= new Repository<Project>(_context);

    public async Task BeginTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction != null)
        {
            throw new InvalidOperationException("A transaction is already active. Commit or rollback the current transaction before starting a new one.");
        }

        _transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
    }

    public async Task CommitTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to commit.");
        }

        try
        {
            // Save changes before committing the transaction
            await _context.SaveChangesAsync(cancellationToken);
            await _transaction.CommitAsync(cancellationToken);
        }
        catch
        {
            await RollbackTransactionAsync(cancellationToken);
            throw;
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task RollbackTransactionAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("No active transaction to rollback.");
        }

        try
        {
            await _transaction.RollbackAsync(cancellationToken);
        }
        finally
        {
            await _transaction.DisposeAsync();
            _transaction = null;
        }
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        if (_transaction == null)
        {
            throw new InvalidOperationException("Cannot save changes without an active transaction. Call BeginTransactionAsync() first.");
        }

        return await _context.SaveChangesAsync(cancellationToken);
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed && disposing)
        {
            // Rollback any active transaction if not committed
            if (_transaction != null)
            {
                try
                {
                    _transaction.Rollback();
                    _transaction.Dispose();
                }
                catch{ } // ex during disposal
            }

            _context.Dispose();
            _disposed = true;
        }
    }
}
