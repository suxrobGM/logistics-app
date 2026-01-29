using Logistics.Application.Abstractions;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class UpdateEmergencyContactHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<UpdateEmergencyContactCommand, Result<EmergencyContactDto>>
{
    public async Task<Result<EmergencyContactDto>> Handle(UpdateEmergencyContactCommand req, CancellationToken ct)
    {
        var contact = await tenantUow.Repository<EmergencyContact>().GetByIdAsync(req.Id, ct);
        if (contact is null)
        {
            return Result<EmergencyContactDto>.Fail("Emergency contact not found.");
        }

        contact.Name = req.Name;
        contact.ContactType = req.ContactType;
        contact.PhoneNumber = req.PhoneNumber;
        contact.Email = req.Email;
        contact.Priority = req.Priority;
        contact.IsActive = req.IsActive;

        tenantUow.Repository<EmergencyContact>().Update(contact);
        await tenantUow.SaveChangesAsync(ct);

        return Result<EmergencyContactDto>.Ok(contact.ToDto());
    }
}
