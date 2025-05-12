using Logistics.Shared.Models;
using MediatR;

namespace Logistics.Application.Queries;

public class CalculatePayrollQuery : IRequest<Result<InvoiceDto>>
{
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public Guid EmployeeId { get; set; }
}
