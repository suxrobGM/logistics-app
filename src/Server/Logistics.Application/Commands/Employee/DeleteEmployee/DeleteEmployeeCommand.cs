using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class DeleteEmployeeCommand : IRequest<Result>
{
    public string UserId { get; set; } = null!;
}
