namespace Logistics.Application.Main.Commands;

public sealed class DeleteTenantCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
}
