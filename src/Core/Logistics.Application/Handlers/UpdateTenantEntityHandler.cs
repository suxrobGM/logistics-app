using Logistics.Application.Abstractions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Handlers;

/// <summary>
///     Base handler for the copy-paste "update a tenant-scoped entity by primary key" pattern:
///     fetch by ID, fail if missing, apply field changes, update, save. Subclasses supply the
///     field copies via <see cref="ApplyChanges" />.
/// </summary>
/// <typeparam name="TCommand">Update command carrying the target <see cref="IHaveId.Id" />.</typeparam>
/// <typeparam name="TEntity">Tenant-scoped entity to update.</typeparam>
internal abstract class UpdateTenantEntityHandler<TCommand, TEntity>(ITenantUnitOfWork tenantUow)
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

        ApplyChanges(req, entity);
        tenantUow.Repository<TEntity>().Update(entity);
        await tenantUow.SaveChangesAsync(ct);
        return Result.Ok();
    }

    /// <summary>Copies the changed fields from the command onto the tracked entity.</summary>
    protected abstract void ApplyChanges(TCommand req, TEntity entity);
}
