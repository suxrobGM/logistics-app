using System.Collections;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Infrastructure.EF.Data;

namespace Logistics.Infrastructure.EF.Persistence;

public class TenantUnitOfWork : ITenantUnityOfWork
{
    private readonly TenantDbContext _tenantDbContext;
    private readonly Hashtable _repositories = new();

    public TenantUnitOfWork(TenantDbContext tenantDbContext)
    {
        _tenantDbContext = tenantDbContext;
    }

    public ITenantRepository2<TEntity> Repository<TEntity>() where TEntity : class, ITenantEntity
    {
        var type = typeof(TEntity).Name;

        if (!_repositories.ContainsKey(type))
        {
            var repositoryType = typeof(TenantRepository2<>);

            var repositoryInstance =
                Activator.CreateInstance(repositoryType
                    .MakeGenericType(typeof(TEntity)), _tenantDbContext);

            _repositories.Add(type, repositoryInstance);
        }

        if (_repositories[type] is not ITenantRepository2<TEntity> repository)
        {
            throw new InvalidOperationException("Could not create a repository");
        }
        
        return repository;
    }

    public Task<int> CommitAsync()
    {
        return _tenantDbContext.SaveChangesAsync();
    }
    
    public void Dispose()
    {
        _tenantDbContext.Dispose();
    }
}
