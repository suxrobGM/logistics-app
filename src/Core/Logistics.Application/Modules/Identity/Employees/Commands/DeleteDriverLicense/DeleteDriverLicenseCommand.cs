
namespace Logistics.Application.Modules.Identity.Employees.Commands;

public class DeleteDriverLicenseCommand : ICommand
{
    public Guid LicenseId { get; set; }
}
