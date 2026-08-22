import type {
  LaneRateFloorDto,
  RateNegotiationDto,
  RateNegotiationStatus,
} from "@logistics/shared/api";

/**
 * The thread is still live: the broker can reply and the agent can still counter. Mirrors
 * `RateNegotiation.IsOpen` on the backend, so a new open status changes one line on each side.
 */
export function isOpenNegotiation(status?: RateNegotiationStatus | null): boolean {
  return status === "awaiting_broker" || status === "broker_replied";
}

/** A rate floor's lane, as country and state codes: `US-TX to US (any state)`. */
export function formatRateFloorLane(floor: LaneRateFloorDto): string {
  const origin = countryStatePlace(floor.originCountry, floor.originState);
  const destination = countryStatePlace(floor.destinationCountry, floor.destinationState);
  return `${origin} to ${destination}`;
}

/** A negotiation's lane, as city and state: `Dallas, TX to Chicago, IL`, or `-` when unknown. */
export function formatNegotiationLane(negotiation: RateNegotiationDto): string {
  const origin = cityStatePlace(negotiation.originCity, negotiation.originState);
  const destination = cityStatePlace(negotiation.destinationCity, negotiation.destinationState);
  return origin && destination ? `${origin} to ${destination}` : "-";
}

/** "Any state" is what a null state column means to the resolver, so it reads that way here too. */
function countryStatePlace(country?: string | null, state?: string | null): string {
  return state ? `${country}-${state}` : `${country} (any state)`;
}

function cityStatePlace(city?: string | null, state?: string | null): string {
  return [city, state].filter(Boolean).join(", ");
}
