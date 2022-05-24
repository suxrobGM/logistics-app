namespace Logistics.EntityFramework.Repositories;

internal class MainUnitOfWork : IMainUnitOfWork
{
    private readonly MainDbContext context;

    public MainUnitOfWork(MainDbContext context)
    {
        this.context = context;
    }

    public Task<int> CommitAsync()
    {
        return context.SaveChangesAsync();
    }
}
