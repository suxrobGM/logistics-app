import {Injectable} from "@angular/core";
import {TruckGeolocationDto} from "@/core/models";
import {TenantService} from "./tenant.service";
import {BaseHubConnection} from "./base-hub-connection";

@Injectable()
export class LiveTrackingService extends BaseHubConnection {
  constructor(tenantService: TenantService) {
    super("live-tracking", tenantService);
  }

  set onReceiveGeolocationData(callback: OnReceiveGeolocationDataCallback) {
    this.hubConnection.on("ReceiveGeolocationData", callback);
  }
}

type OnReceiveGeolocationDataCallback = (geolocation: TruckGeolocationDto) => void;
