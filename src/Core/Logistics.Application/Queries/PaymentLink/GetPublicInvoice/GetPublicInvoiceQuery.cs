using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets invoice details via a public payment link token.
/// This query does not require authentication.
/// </summary>
public record GetPublicInvoiceQuery(Guid TenantId, string Token) : IAppRequest<Result<PublicInvoiceDto>>;
