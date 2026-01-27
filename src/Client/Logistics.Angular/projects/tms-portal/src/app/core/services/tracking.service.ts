import { Injectable } from "@angular/core";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { BaseHubConnection } from "./base-hub-connection";

@Injectable({ providedIn: "root" })
export class TrackingService extends BaseHubConnection {
  constructor() {
    super("tracking");
  }

  set onReceiveGeolocationData(callback: OnReceiveGeolocationDataCallback) {
    this.hubConnection.on("ReceiveGeolocationData", callback);
  }
}

type OnReceiveGeolocationDataCallback = (geolocation: TruckGeolocationDto) => void;
