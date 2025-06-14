import { Injectable, inject } from "@angular/core";
import {TruckGeolocationDto} from "@/core/api/models";
import {BaseHubConnection} from "./base-hub-connection";
import {TenantService} from "./tenant.service";

@Injectable({providedIn: "root"})
export class LiveTrackingService extends BaseHubConnection {
  constructor() {
    const tenantService = inject(TenantService);

    super("live-tracking", tenantService);
  }

  set onReceiveGeolocationData(callback: OnReceiveGeolocationDataCallback) {
    this.hubConnection.on("ReceiveGeolocationData", callback);
  }
}

type OnReceiveGeolocationDataCallback = (geolocation: TruckGeolocationDto) => void;
