namespace Logistics.Application.Main.Queries;

public sealed class GetUserByIdQuery : RequestBase<ResponseResult<UserDto>>
{
    public string? Id { get; set; }
}
