namespace Logistics.Application.Contracts.Queries;

public sealed class UserExistsQuery : RequestBase<DataResult<bool>>
{
    public string? ExternalId { get; set; }
}
