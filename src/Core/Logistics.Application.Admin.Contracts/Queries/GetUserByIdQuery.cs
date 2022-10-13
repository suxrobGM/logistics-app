namespace Logistics.Application.Admin.Queries;

public sealed class GetUserByIdQuery : RequestBase<ResponseResult<UserDto>>
{
    public string? Id { get; set; }
}
