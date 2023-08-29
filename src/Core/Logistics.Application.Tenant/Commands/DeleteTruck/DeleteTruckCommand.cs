namespace Logistics.Application.Tenant.Commands;

public sealed class DeleteTruckCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
}
