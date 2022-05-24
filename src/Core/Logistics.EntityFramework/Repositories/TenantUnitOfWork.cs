namespace Logistics.EntityFramework.Repositories;

internal class TenantUnitOfWork : ITenantUnitOfWork
{
    private readonly TenantDbContext _context;

    public TenantUnitOfWork(TenantDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync()
    {
        return _context.SaveChangesAsync();
    }
}
