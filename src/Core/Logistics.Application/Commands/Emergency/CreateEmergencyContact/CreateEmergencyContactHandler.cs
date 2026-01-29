using Logistics.Application.Abstractions;
using Logistics.Domain.Entities;
using Logistics.Domain.Entities.Safety;
using Logistics.Domain.Persistence;
using Logistics.Mappings;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

internal sealed class CreateEmergencyContactHandler(ITenantUnitOfWork tenantUow)
    : IAppRequestHandler<CreateEmergencyContactCommand, Result<EmergencyContactDto>>
{
    public async Task<Result<EmergencyContactDto>> Handle(CreateEmergencyContactCommand req, CancellationToken ct)
    {
        if (req.EmployeeId.HasValue)
        {
            var employee = await tenantUow.Repository<Employee>().GetByIdAsync(req.EmployeeId.Value, ct);
            if (employee is null)
            {
                return Result<EmergencyContactDto>.Fail("Employee not found.");
            }
        }

        var contact = new EmergencyContact
        {
            EmployeeId = req.EmployeeId,
            Name = req.Name,
            ContactType = req.ContactType,
            PhoneNumber = req.PhoneNumber,
            Email = req.Email,
            Priority = req.Priority,
            IsActive = true
        };

        await tenantUow.Repository<EmergencyContact>().AddAsync(contact, ct);
        await tenantUow.SaveChangesAsync(ct);

        return Result<EmergencyContactDto>.Ok(contact.ToDto());
    }
}
