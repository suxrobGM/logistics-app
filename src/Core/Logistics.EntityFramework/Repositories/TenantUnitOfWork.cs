namespace Logistics.EntityFramework.Repositories;

internal class TenantUnitOfWork : GenericUnitOfWork<TenantDbContext>, ITenantUnitOfWork
{
    public TenantUnitOfWork(TenantDbContext context) : base(context)
    {
    }
}
