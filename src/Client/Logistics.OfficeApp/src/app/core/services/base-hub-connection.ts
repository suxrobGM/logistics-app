import {HttpTransportType, HubConnection, HubConnectionBuilder} from "@microsoft/signalr";
import {environment} from "@/env";
import {TenantService} from "./tenant.service";

export abstract class BaseHubConnection {
  protected readonly hubConnection: HubConnection;
  private isConnected: boolean;

  constructor(
    private hubName: string,
    private tenantService: TenantService
  ) {
    this.isConnected = false;
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiHost}/hubs/${hubName}`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .build();
  }

  async connect(): Promise<void> {
    if (this.isConnected) {
      return;
    }

    try {
      const tenant = this.tenantService.getTenantData();

      if (!tenant) {
        console.error(`Failed to connect to the ${this.hubName} hub, tenant ID is null`);
        return;
      }

      await this.hubConnection.start();
      await this.hubConnection.invoke("RegisterTenant", tenant.id);
      this.isConnected = true;
    } catch (error) {
      console.error(`Failed to connect to the ${this.hubName} hub`, error);
      this.isConnected = false;
    }
  }

  async disconnect(): Promise<void> {
    if (!this.isConnected) {
      return;
    }

    try {
      const tenant = this.tenantService.getTenantData();

      if (!tenant) {
        console.error(`Failed to disconnect from the ${this.hubName} hub, tenant ID is null`);
        return;
      }

      await this.hubConnection.invoke("UnregisterTenant", tenant.id);
      await this.hubConnection.stop();
      this.isConnected = false;
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
