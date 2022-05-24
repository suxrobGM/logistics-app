namespace Logistics.EntityFramework.Repositories;

internal class TenantUnitOfWork : ITenantUnitOfWork
{
    private readonly TenantDbContext context;

    public TenantUnitOfWork(TenantDbContext context)
    {
        this.context = context;
    }

    public Task<int> CommitAsync()
    {
        return context.SaveChangesAsync();
    }
}
