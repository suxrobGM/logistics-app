using Logistics.Application.Common;
using Logistics.Shared;

namespace Logistics.Application.Admin.Commands;

public class UpdateUserCommand : Request<ResponseResult>
{
    public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
