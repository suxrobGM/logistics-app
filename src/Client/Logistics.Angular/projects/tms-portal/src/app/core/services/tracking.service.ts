import { Injectable } from "@angular/core";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { BaseHubConnection } from "./base-hub-connection";

/**
 * Service for managing real-time tracking of trucks via SignalR.
 */
@Injectable({ providedIn: "root" })
export class TrackingService extends BaseHubConnection {
  readonly geolocationReceived$ = this.event<TruckGeolocationDto>("ReceiveGeolocationData");

  constructor() {
    super("tracking");
  }
}
