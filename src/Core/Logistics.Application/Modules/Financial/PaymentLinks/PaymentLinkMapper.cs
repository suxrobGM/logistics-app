using Logistics.Domain.Entities;
using Logistics.Shared.Models;
using Riok.Mapperly.Abstractions;

namespace Logistics.Application.Modules.Financial.PaymentLinks;

[Mapper]
public static partial class PaymentLinkMapper
{
    [MapperIgnoreSource(nameof(PaymentLink.Invoice))]
    [MapperIgnoreTarget(nameof(PaymentLinkDto.Url))]
    private static partial PaymentLinkDto MapPaymentLink(PaymentLink entity);

    extension(PaymentLink entity)
    {
        public PaymentLinkDto ToDto(string? url = null)
        {
            var dto = MapPaymentLink(entity);
            return dto with { Url = url };
        }
    }
}
