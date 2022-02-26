namespace Logistics.Application.Contracts.Queries;

public sealed class UserExistsQuery : RequestBase<DataResult<UserDto>>
{
    public string? ExternalId { get; set; }
}
