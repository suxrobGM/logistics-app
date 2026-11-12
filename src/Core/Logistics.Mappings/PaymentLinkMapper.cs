using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentLinkMapper
{
    extension(PaymentLink entity)
    {
        public PaymentLinkDto ToDto(string? baseUrl = null)
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
                Url = baseUrl != null ? $"{baseUrl}/pay/{entity.Token}" : null
            };
        }
    }

    extension(IEnumerable<PaymentLink> entities)
    {
        public IEnumerable<PaymentLinkDto> ToDto(string? baseUrl = null) =>
            entities.Select(e => e.ToDto(baseUrl));
    }
}
