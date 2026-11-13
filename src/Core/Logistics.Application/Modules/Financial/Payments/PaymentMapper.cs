using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Financial.Payments;

[Mapper]
public static partial class PaymentMapper
{
    [MapperIgnoreSource(nameof(Payment.Invoice))]
    public static partial PaymentDto ToDto(this Payment entity);
}
