using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Persistence;

internal sealed class MasterUnitOfWork
    : UnitOfWork<IMasterEntity>, IMasterUnitOfWork
{
    private readonly MasterDbContext _db;
    private readonly IServiceProvider _services;

    public MasterUnitOfWork(MasterDbContext db, IServiceProvider services) : base(db)
    {
        _db = db;
        _services = services;
    }

    // Strongly typed repos (hide base with 'new')
    public new IMasterRepository<TEntity, Guid> Repository<TEntity>()
        where TEntity : class, IEntity<Guid>, IMasterEntity
    {
        return (IMasterRepository<TEntity, Guid>)base.Repository<TEntity>();
    }

    public new IMasterRepository<TEntity, TKey> Repository<TEntity, TKey>()
        where TEntity : class, IEntity<TKey>, IMasterEntity
    {
        return (IMasterRepository<TEntity, TKey>)base.Repository<TEntity, TKey>();
    }

    protected override IRepository<TEntity, TKey> CreateRepository<TEntity, TKey>()
    {
        // MasterRepository<TEntity, TKey>(MasterDbContext db)
        return ActivatorUtilities.CreateInstance<MasterRepository<TEntity, TKey>>(_services, _db);
    }
}
