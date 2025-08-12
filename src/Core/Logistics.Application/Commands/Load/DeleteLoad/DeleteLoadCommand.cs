using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteLoadCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
