using Logistics.Domain.ValueObjects;
using Logistics.Shared.Models;

namespace Logistics.Mappings;

public static class MoneyMapper
{
    public static MoneyDto ToDto(this Money entity)
    {
        return new MoneyDto
        {
            Amount = entity.Amount,
            Currency = entity.Currency,
        };
    }

    public static Money ToEntity(this MoneyDto dto)
    {
        return new Money 
        {
            Amount = dto.Amount,
            Currency = dto.Currency,
        };
    }
}