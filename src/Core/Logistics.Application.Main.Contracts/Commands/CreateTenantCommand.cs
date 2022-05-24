namespace Logistics.Application.Contracts.Commands;

public sealed class CreateTenantCommand : RequestBase<DataResult>
{
    public string? Name { get; set; }
    public string? DisplayName { get; set; }
    public string? ConnectionString { get; set; }
}
