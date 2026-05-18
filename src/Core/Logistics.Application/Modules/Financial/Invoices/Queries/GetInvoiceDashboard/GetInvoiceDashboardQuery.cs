using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Financial.Invoices.Queries;

/// <summary>
/// Gets invoice dashboard statistics including counts and amounts by status.
/// </summary>
public record GetInvoiceDashboardQuery : IQuery<Result<InvoiceDashboardDto>>;
