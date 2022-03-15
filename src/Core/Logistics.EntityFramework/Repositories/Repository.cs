using Logistics.Domain.Specifications;
using System.Linq.Expressions;

namespace Logistics.EntityFramework.Repositories;

public class Repository<TEntity> : IRepository<TEntity>
    where TEntity : class, IAggregateRoot
{
    private readonly DatabaseContext context;

    public Repository(
        DatabaseContext context,
        IUnitOfWork unitOfWork)
    {
        this.context = context;
        UnitOfWork = unitOfWork;
    }

    public IUnitOfWork UnitOfWork { get; }

    public async Task<TEntity?> GetAsync(object id)
    {
        var entity = await context.Set<TEntity>().FindAsync(id);
        return entity;
    }

    public Task<TEntity?> GetAsync(Expression<Func<TEntity, bool>> predicate)
    {
        return context.Set<TEntity>().FirstOrDefaultAsync(predicate);
    }

    public Task<TEntity?> GetAsync(ISpecification<TEntity> specification)
    {
        return GetAsync(specification.Criteria);
    }

    public async Task<IList<TEntity>> GetListAsync(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate == null ? await context.Set<TEntity>().ToListAsync()
            : await context.Set<TEntity>().Where(predicate).ToListAsync();
    }

    public Task<IList<TEntity>> GetListAsync(ISpecification<TEntity> specification)
    {
        return GetListAsync(specification.Criteria);
    }

    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = default!)
    {
        return predicate != null ? context.Set<TEntity>().Where(predicate)
            : context.Set<TEntity>();
    }

    public IQueryable<TEntity> GetQuery(ISpecification<TEntity> specification)
    {
        return GetQuery(specification.Criteria);
    }

    public async Task AddAsync(TEntity entity)
    {
        await context.Set<TEntity>().AddAsync(entity);
    }

    public void Update(TEntity entity)
    {
        context.Set<TEntity>().Attach(entity);
        context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(object id)
    {
        if (id is null)
            return;

        var entity = context.Set<TEntity>().Find(id);
        Delete(entity);
    }

    public void Delete(TEntity? entity)
    {
        if (entity == null)
            return;

        context.Set<TEntity>().Remove(entity);
    }
}