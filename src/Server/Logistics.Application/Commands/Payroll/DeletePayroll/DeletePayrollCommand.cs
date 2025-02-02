using Logistics.Shared;
using MediatR;

namespace Logistics.Application.Commands;

public class DeletePayrollCommand : IRequest<Result>
{
    public string Id { get; set; } = null!;
}
