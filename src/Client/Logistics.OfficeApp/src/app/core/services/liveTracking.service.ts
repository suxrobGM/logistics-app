import {Injectable} from '@angular/core';
import {HttpTransportType, HubConnection, HubConnectionBuilder} from '@microsoft/signalr';
import {AppConfig} from '@configs';
import {TruckGeolocation} from '@core/models';
import {TenantService} from './tenant.service';


@Injectable()
export class LiveTrackingService {
  private readonly hubConnection: HubConnection;
  private isConnected: boolean;

  constructor(private tenantService: TenantService) {
    this.isConnected = false;
    this.hubConnection = new HubConnectionBuilder()
        .withUrl(`${AppConfig.apiHost}/hubs/live-tracking`, {
          skipNegotiation: true,
          transport: HttpTransportType.WebSockets,
        })
        .build();
  }

  set onReceiveGeolocationData(callback: OnReceiveGeolocationDataCallback) {
    this.hubConnection.on('ReceiveGeolocationData', callback);
  }

  async connect(): Promise<void> {
    if (this.isConnected) {
      return;
    }

    try {
      const tenant = this.tenantService.getTenantData();

      if (!tenant) {
        console.error('Failed to connect to the LiveTracking hub, tenant ID is null');
        return;
      }

      await this.hubConnection.start();
      await this.hubConnection.invoke('RegisterTenant', tenant.id);
      this.isConnected = true;
    } catch (error) {
      console.error('Failed to connect to the LiveTracking hub', error);
      this.isConnected = false;
    }
  }

  async disconnect() {
    if (!this.isConnected) {
      return;
    }

    try {
      const tenant = this.tenantService.getTenantData();

      if (!tenant) {
        console.error('Failed to disconnect from the LiveTracking hub, tenant ID is null');
        return;
      }

      await this.hubConnection.invoke('UnregisterTenant', tenant.id);
      await this.hubConnection.stop();
      this.isConnected = false;
    } catch (error) {
      console.error('Failed to disconnect from the LiveTracking hub', error);
    }
  }
}

type OnReceiveGeolocationDataCallback = (geolocation: TruckGeolocation) => void;
