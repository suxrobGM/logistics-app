using Logistics.Application.Abstractions;
using Logistics.Shared.Models;

namespace Logistics.Application.Modules.Integrations.FuelCards.Queries;

public class GetFuelCardsQuery : IQuery<Result<List<FuelCardDto>>>
{
}
