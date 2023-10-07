using Logistics.Shared.Enums;

namespace Logistics.Application.Tenant.Commands;

public class ConfirmLoadStatusCommand : Request<ResponseResult>
{
    public string? LoadId { get; set; }
    public LoadStatus? LoadStatus { get; set; }
}
