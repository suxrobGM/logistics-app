namespace Logistics.EntityFramework.Repositories;

internal class MainUnitOfWork : GenericUnitOfWork<MainDbContext>, IMainUnitOfWork
{
    public MainUnitOfWork(MainDbContext context) : base(context)
    {
    }
}
