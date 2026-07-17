using Logistics.Application.Abstractions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Handlers;

/// <summary>
///     Base handler for the copy-paste "delete a tenant-scoped entity by primary key" pattern:
///     fetch by ID, fail if missing, delete, save.
/// </summary>
/// <typeparam name="TCommand">Delete command carrying the target <see cref="IHaveId.Id" />.</typeparam>
/// <typeparam name="TEntity">Tenant-scoped entity to delete.</typeparam>
internal abstract class DeleteTenantEntityHandler<TCommand, TEntity>(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<TCommand, Result>
    where TCommand : ICommand<Result>, IHaveId
    where TEntity : class, IEntity<Guid>, ITenantEntity
{
    public async Task<Result> Handle(TCommand req, CancellationToken ct)
    {
        var entity = await tenantUow.Repository<TEntity>().GetByIdAsync(req.Id, ct);

        if (entity is null)
        {
            return Result.Fail($"Could not find a {EntityName.Humanize<TEntity>()} with ID '{req.Id}'");
        }

        tenantUow.Repository<TEntity>().Delete(entity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
