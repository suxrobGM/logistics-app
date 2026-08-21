import type { RateNegotiationStatus } from "@logistics/shared/api";

/**
 * The thread is still live: the broker can reply and the agent can still counter. Mirrors
 * `RateNegotiation.IsOpen` on the backend, so a new open status changes one line on each side.
 */
export function isOpenNegotiation(status?: RateNegotiationStatus | null): boolean {
  return status === "awaiting_broker" || status === "broker_replied";
}
