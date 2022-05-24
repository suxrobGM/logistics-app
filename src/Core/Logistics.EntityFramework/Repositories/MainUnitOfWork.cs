namespace Logistics.EntityFramework.Repositories;

internal class MainUnitOfWork : IMainUnitOfWork
{
    private readonly MainDbContext _context;

    public MainUnitOfWork(MainDbContext context)
    {
        _context = context;
    }

    public Task<int> CommitAsync()
    {
        return _context.SaveChangesAsync();
    }
}
