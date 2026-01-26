using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.Persistence.Data;
using Microsoft.Extensions.DependencyInjection;

namespace Logistics.Infrastructure.Persistence.Repositories;

internal sealed class MasterUnitOfWork
    : UnitOfWork<IMasterEntity>, IMasterUnitOfWork
{
    private readonly MasterDbContext db;
    private readonly IServiceProvider services;

    public MasterUnitOfWork(MasterDbContext db, IServiceProvider services) : base(db)
    {
        this.db = db;
        this.services = services;
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
        return ActivatorUtilities.CreateInstance<MasterRepository<TEntity, TKey>>(services, db);
    }
}
