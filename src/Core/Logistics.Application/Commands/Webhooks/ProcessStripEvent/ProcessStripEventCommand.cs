using Logistics.Shared.Models;

using MediatR;

namespace Logistics.Application.Commands;

public class ProcessStripEventCommand : IRequest<Result>
{
    public string? RequestBodyJson { get; set; }
    public string? StripeSignature { get; set; }
}
