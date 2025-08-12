using Logistics.Application.Abstractions;

namespace Logistics.Application.Commands;

public class DeletePaymentCommand : IAppRequest
{
    public Guid Id { get; set; }
}
