using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Commands;

/// <summary>
/// Revokes a payment link, making it invalid.
/// </summary>
public record RevokePaymentLinkCommand(Guid PaymentLinkId) : IAppRequest<Result>;
