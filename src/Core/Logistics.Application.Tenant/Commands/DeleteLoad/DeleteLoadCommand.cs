namespace Logistics.Application.Tenant.Commands;

public sealed class DeleteLoadCommand : RequestBase<ResponseResult>
{
    public string? Id { get; set; }
}
