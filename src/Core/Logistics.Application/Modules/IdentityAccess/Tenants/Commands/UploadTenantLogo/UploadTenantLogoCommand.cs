using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.IdentityAccess.Tenants.Commands;

public class UploadTenantLogoCommand : ICommand<Result<string>>
{
    public Guid TenantId { get; set; }
    public required Stream FileContent { get; set; }
    public required string FileName { get; set; }
    public required string ContentType { get; set; }
    public long FileSizeBytes { get; set; }
}
