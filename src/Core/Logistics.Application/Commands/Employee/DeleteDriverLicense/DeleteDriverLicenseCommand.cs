using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeleteDriverLicenseCommand : IAppRequest
{
    public Guid LicenseId { get; set; }
}
