using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Admin.Commands;

public class UpdateUserCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? PhoneNumber { get; set; }
}
