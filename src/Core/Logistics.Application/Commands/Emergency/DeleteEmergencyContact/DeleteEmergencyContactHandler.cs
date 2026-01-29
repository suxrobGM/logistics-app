using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class DeleteEmergencyContactHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<DeleteEmergencyContactCommand, Result>
{
    public async Task<Result> Handle(DeleteEmergencyContactCommand req, CancellationToken ct)
    {
        var contact = await tenantUow.Repository<EmergencyContact>().GetByIdAsync(req.Id, ct);
        if (contact is null)
        {
            return Result.Fail("Emergency contact not found.");
        }

        tenantUow.Repository<EmergencyContact>().Delete(contact);
        await tenantUow.SaveChangesAsync(ct);

        return Result.Ok();
    }
}
