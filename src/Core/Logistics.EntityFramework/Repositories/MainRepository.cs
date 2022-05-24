using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

internal class MainRepository<TEntity> : IMainRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
    private readonly MainDbContext _context;

    public MainRepository(
        MainDbContext context,
        IMainUnitOfWork unitOfWork)
    {
        _context = context;
        UnitOfWork = unitOfWork;
    }

    public IMainUnitOfWork UnitOfWork { get; }

    public async Task<TEntity?> GetAsync(object id)
    {
        var entity = await _context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate == null ? await _context.Set<TEntity>().ToListAsync()
            : await _context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate != null ? _context.Set<TEntity>().Where(predicate)
            : _context.Set<TEntity>();
    }

    public async Task AddAsync(TEntity entity)
    {
        await _context.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(object id)
    {
        if (id is null)
            return;

        var entity = _context.Set<TEntity>().Find(id);
        Delete(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity == null)
            return;

        _context.Set<TEntity>().Remove(entity);
    }
}