using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteEmployeeCommand : IRequest<Result>
{
    public Guid UserId { get; set; }
}
