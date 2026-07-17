using Logistics.Application.Abstractions;
using Logistics.Domain.Core;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Handlers;

/// <summary>
///     Base handler for the copy-paste "delete a master-scoped entity by primary key" pattern:
///     fetch by ID, fail if missing, delete, save.
/// </summary>
/// <typeparam name="TCommand">Delete command carrying the target <see cref="IHaveId.Id" />.</typeparam>
/// <typeparam name="TEntity">Master-scoped entity to delete.</typeparam>
internal abstract class DeleteMasterEntityHandler<TCommand, TEntity>(IMasterUnitOfWork masterUow)
    : IAppRequestHandler<TCommand, Result>
    where TCommand : ICommand<Result>, IHaveId
    where TEntity : class, IEntity<Guid>, IMasterEntity
{
    public async Task<Result> Handle(TCommand req, CancellationToken ct)
    {
        var entity = await masterUow.Repository<TEntity>().GetByIdAsync(req.Id, ct);

        if (entity is null)
        {
            return Result.Fail($"Could not find a {EntityName.Humanize<TEntity>()} with ID '{req.Id}'");
        }

        masterUow.Repository<TEntity>().Delete(entity);
        await masterUow.SaveChangesAsync(ct);
        return Result.Ok();
    }
}
