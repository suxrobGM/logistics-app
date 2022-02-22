namespace Logistics.EntityFramework.Repositories;

internal class UnitOfWork : IUnitOfWork
{
    private readonly DatabaseContext context;

    public UnitOfWork(DatabaseContext context)
    {
        this.context = context;
    }

    public Task<int> CommitAsync()
    {
        return context.SaveChangesAsync();
    }
}
