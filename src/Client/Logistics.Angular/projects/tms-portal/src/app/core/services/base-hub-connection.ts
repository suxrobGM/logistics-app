import { inject } from "@angular/core";
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr";
import { environment } from "@/env";
import { TenantService } from "./tenant.service";

export interface HubConnectionOptions {
  /** JWT for authorized hubs; SignalR sends it as the access_token query parameter. */
  accessTokenFactory?: () => string | Promise<string>;

  /**
   * Whether to run the RegisterTenant/UnregisterTenant handshake. Authorized hubs derive identity
   * from JWT claims and skip it. Default true.
   */
  registerTenant?: boolean;
}

export abstract class BaseHubConnection {
  private readonly tenantService = inject(TenantService);
  private readonly registerTenant: boolean;
  protected readonly hubConnection: HubConnection;

  constructor(
    private readonly hubName: string,
    options: HubConnectionOptions = {},
  ) {
    this.registerTenant = options.registerTenant ?? true;
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/${hubName}`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
        accessTokenFactory: options.accessTokenFactory,
      })
      .withAutomaticReconnect()
      .build();
  }

  get isConnected(): boolean {
    return this.hubConnection.state === HubConnectionState.Connected;
  }

  async connect(): Promise<void> {
    // Only start if in Disconnected state
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      return;
    }

    try {
      await this.hubConnection.start();

      if (this.registerTenant) {
        const tenant = this.tenantService.getTenantData();
        if (!tenant) {
          console.error(`Failed to connect to the ${this.hubName} hub, tenant ID is null`);
          return;
        }
        await this.hubConnection.invoke("RegisterTenant", tenant.id);
      }
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
      if (this.registerTenant) {
        const tenant = this.tenantService.getTenantData();
        if (tenant) {
          await this.hubConnection.invoke("UnregisterTenant", tenant.id);
        }
      }

      await this.hubConnection.stop();
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
