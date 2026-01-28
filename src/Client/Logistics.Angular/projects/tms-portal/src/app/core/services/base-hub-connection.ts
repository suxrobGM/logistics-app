import { inject } from "@angular/core";
import { HttpTransportType, HubConnection, HubConnectionBuilder, HubConnectionState } from "@microsoft/signalr";
import { environment } from "@/env";
import { TenantService } from "./tenant.service";

export abstract class BaseHubConnection {
  private readonly tenantService = inject(TenantService);
  protected readonly hubConnection: HubConnection;

  constructor(private readonly hubName: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/${hubName}`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
      })
      .build();
  }

  async connect(): Promise<void> {
    // Only start if in Disconnected state
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
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
    } catch (error) {
      console.error(`Failed to connect to the ${this.hubName} hub`, error);
    }
  }

  async disconnect(): Promise<void> {
    // Only disconnect if in Connected state
    if (this.hubConnection.state !== HubConnectionState.Connected) {
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
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
