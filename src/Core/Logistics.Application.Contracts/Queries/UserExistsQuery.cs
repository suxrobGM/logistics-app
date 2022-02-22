namespace Logistics.Application.Contracts.Queries;

public class UserExistsQuery : RequestBase<DataResult<bool>>
{
    public string? ExternalId { get; set; }
}
