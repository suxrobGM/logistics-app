using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteDriverLicenseCommand : ICommand
{
    public Guid LicenseId { get; set; }
}
