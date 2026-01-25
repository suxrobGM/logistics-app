using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Queries;

/// <summary>
/// Gets invoice dashboard statistics including counts and amounts by status.
/// </summary>
public record GetInvoiceDashboardQuery : IAppRequest<Result<InvoiceDashboardDto>>;
