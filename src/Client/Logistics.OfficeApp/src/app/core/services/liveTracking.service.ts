import {Injectable} from '@angular/core';
import {TruckGeolocation} from '@core/models';
import {TenantService} from './tenant.service';
import {BaseHubConnection} from './baseHubConnection';


@Injectable()
export class LiveTrackingService extends BaseHubConnection {
  constructor(tenantService: TenantService) {
    super('live-tracking', tenantService);
  }

  set onReceiveGeolocationData(callback: OnReceiveGeolocationDataCallback) {
    this.hubConnection.on('ReceiveGeolocationData', callback);
  }
}

type OnReceiveGeolocationDataCallback = (geolocation: TruckGeolocation) => void;
