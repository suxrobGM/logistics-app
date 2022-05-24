namespace Logistics.Application.Contracts.Commands;

public sealed class DeleteTenantCommand : RequestBase<DataResult>
{
    public string? Id { get; set; }
}
