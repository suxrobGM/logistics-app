using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class DeleteInvoiceCommand : IRequest<Result>
{
    public Guid Id { get; set; }
}
