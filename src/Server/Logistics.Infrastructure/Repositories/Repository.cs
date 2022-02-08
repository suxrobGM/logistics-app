using System.Linq.Expressions;
using Logistics.Domain;
using Logistics.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Logistics.Infrastructure.Repositories;

public class Repository<TEntity> : IRepository<TEntity> 
    where TEntity: class, IAggregateRoot
{
    private readonly DatabaseContext _context;

    protected Repository(DatabaseContext context,
        IUnitOfWork unitOfWork)
    {
        UnitOfWork = unitOfWork;
        _context = context;
    }
    
    public IUnitOfWork UnitOfWork { get; }
    
    public TEntity GetById(string id)
    {
        return Get(i => i.Id == id);
    }

    public TEntity Get(Expression<Func<TEntity, bool>> predicate)
    {
        return _context.Set<TEntity>().FirstOrDefault(predicate);
    }

    public List<TEntity> GetList(Expression<Func<TEntity, bool>> predicate = null)
    {
        return predicate == null ? _context.Set<TEntity>().ToList() 
            : _context.Set<TEntity>().Where(predicate).ToList();
    }

    public IQueryable<TEntity> GetQuery(Expression<Func<TEntity, bool>> predicate = null)
    {
        return predicate != null ? _context.Set<TEntity>().Where(predicate) 
            : _context.Set<TEntity>();
    }

    public void Add(TEntity entity)
    {
        _context.Set<TEntity>().Add(entity);
    }

    public void Update(TEntity entity)
    {
        _context.Set<TEntity>().Attach(entity);
        _context.Entry(entity).State = EntityState.Modified;
    }

    public void Delete(string id)
    {
        if (string.IsNullOrEmpty(id))
            return;

        var entity = _context.Set<TEntity>().Find(id);
        Delete(entity);
    }

    public void Delete(TEntity entity)
    {
        if (entity == null)
            return;

        _context.Set<TEntity>().Remove(entity);
    }
}