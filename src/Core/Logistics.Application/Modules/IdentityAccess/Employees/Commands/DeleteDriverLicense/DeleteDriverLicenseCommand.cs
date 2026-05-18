using Logistics.Application.Abstractions;

namespace Logistics.Application.Modules.IdentityAccess.Employees.Commands;

public class DeleteDriverLicenseCommand : ICommand
{
    public Guid LicenseId { get; set; }
}
