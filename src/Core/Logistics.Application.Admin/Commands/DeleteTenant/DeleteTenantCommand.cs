namespace Logistics.Application.Admin.Commands;

public sealed class DeleteTenantCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
}
