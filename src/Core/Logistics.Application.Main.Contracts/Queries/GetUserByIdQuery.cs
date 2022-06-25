namespace Logistics.Application.Contracts.Queries;

public sealed class GetUserByIdQuery : RequestBase<DataResult<UserDto>>
{
    public string? Id { get; set; }
}
