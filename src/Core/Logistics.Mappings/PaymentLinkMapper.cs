using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentLinkMapper
{
    extension(PaymentLink entity)
    {
        public PaymentLinkDto ToDto(string? url = null)
        {
            return new PaymentLinkDto
            {
                Id = entity.Id,
                Token = entity.Token,
                InvoiceId = entity.InvoiceId,
                ExpiresAt = entity.ExpiresAt,
                IsActive = entity.IsActive,
                CreatedByUserId = entity.CreatedByUserId,
                AccessCount = entity.AccessCount,
                LastAccessedAt = entity.LastAccessedAt,
                IsValid = entity.IsValid,
                CreatedAt = entity.CreatedAt,
                Url = url
            };
        }
    }
}
