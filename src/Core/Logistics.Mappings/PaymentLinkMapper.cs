using Logistics.Domain.Entities;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class PaymentLinkMapper
{
    public static PaymentLinkDto ToDto(this PaymentLink entity, string? baseUrl = null)
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

    public static IEnumerable<PaymentLinkDto> ToDto(this IEnumerable<PaymentLink> entities, string? baseUrl = null) =>
        entities.Select(e => e.ToDto(baseUrl));
}
