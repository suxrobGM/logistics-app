using Logistics.Shared.Enums;
using MediatR;

namespace Logistics.Application.Tenant.Commands;

public class UpdateInvoiceCommand : IRequest<ResponseResult>
{
    public string Id { get; set; } = default!;
    public string? CompanyName { get; set; }
    public string? CompanyAddress { get; set; }
    public string CustomerId { get; set; } = default!;
    public string LoadId { get; set; } = default!;
    public PaymentMethod PaymentMethod { get; set; }
    public decimal PaymentAmount { get; set; }
    
    public PaymentMethod? Method { get; set; }
    public decimal? Amount { get; set; }
    public PaymentStatus? Status { get; set; }
    public PaymentFor? PaymentFor { get; set; }
    public string? Comment { get; set; }
}
