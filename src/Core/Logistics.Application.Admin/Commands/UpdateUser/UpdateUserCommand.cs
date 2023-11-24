using Logistics.Application.Core;
using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateUserCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
