import { computed, DestroyRef, inject, signal } from "@angular/core";
import { getAccessToken } from "@logistics/shared";
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr";
import { Observable, Subject } from "rxjs";
import { environment } from "@/env";
import { TenantService } from "./tenant.service";

export type HubConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting";

export interface HubConnectionOptions {
  /** Overrides the portal access token. */
  accessTokenFactory?: () => string | Promise<string>;
}

/** How long a hub lingers after its last consumer releases, so route changes don't churn it. */
const DisconnectLingerMs = 5000;

/** Shared SignalR connection with reference-counted lifecycle and reconnect-safe group membership. */
export abstract class BaseHubConnection {
  protected readonly tenantService = inject(TenantService);
  private readonly state = signal<HubConnectionStatus>("disconnected");
  protected readonly hubConnection: HubConnection;

  private claims = 0;
  private lingerTimer: ReturnType<typeof setTimeout> | null = null;
  /** Serializes connection lifecycle changes. */
  private lifecycle: Promise<void> = Promise.resolve();

  /** Group joins replayed after reconnecting with a new connection ID. */
  private readonly groupJoins = new Map<string, { method: string; args: unknown[] }>();

  /** Live connection status for offline/reconnecting UI. */
  readonly connectionState = this.state.asReadonly();

  /** Whether live updates are currently unavailable. */
  readonly realtimeDown = computed(
    () => this.state() === "disconnected" || this.state() === "reconnecting",
  );

  constructor(
    private readonly hubName: string,
    options: HubConnectionOptions = {},
  ) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/${hubName}`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
        accessTokenFactory: options.accessTokenFactory ?? (() => getAccessToken("tmsportal") ?? ""),
      })
      .withAutomaticReconnect()
      .build();

    this.hubConnection.onreconnecting(() => this.state.set("reconnecting"));
    this.hubConnection.onreconnected(() => {
      this.state.set("connected");
      void this.rejoinGroups();
    });
    this.hubConnection.onclose(() => this.state.set("disconnected"));
  }

  get isConnected(): boolean {
    return this.hubConnection.state === HubConnectionState.Connected;
  }

  /** Acquires and starts the connection for a consumer's lifetime. */
  acquire(destroyRef?: DestroyRef): Promise<void> {
    if (this.lingerTimer) {
      clearTimeout(this.lingerTimer);
      this.lingerTimer = null;
    }
    this.claims++;
    destroyRef?.onDestroy(() => this.release());
    return this.enqueue(() => this.start());
  }

  /** Releases a claim and stops the connection after the final consumer leaves. */
  release(): void {
    if (this.claims > 0) {
      this.claims--;
    }
    if (this.claims > 0 || this.lingerTimer) {
      return;
    }
    this.lingerTimer = setTimeout(() => {
      this.lingerTimer = null;
      void this.enqueue(() => this.stop());
    }, DisconnectLingerMs);
  }

  /** Joins a group now and after each reconnect. */
  protected async joinGroup(key: string, method: string, ...args: unknown[]): Promise<void> {
    this.groupJoins.set(key, { method, args });
    await this.invokeGroup(method, args);
  }

  /** Leaves a {@link joinGroup} membership and stops replaying it. */
  protected async leaveGroup(key: string, method: string, ...args: unknown[]): Promise<void> {
    this.groupJoins.delete(key);
    await this.invokeGroup(method, args);
  }

  /** Registers a server event once and exposes its payload as an observable. */
  protected event<T>(method: string): Observable<T> {
    return this.mappedEvent(method, (payload: T) => payload);
  }

  /** {@link event} for hub methods that send several arguments. */
  protected mappedEvent<TArgs extends unknown[], T>(
    method: string,
    project: (...args: TArgs) => T,
  ): Observable<T> {
    const subject = new Subject<T>();
    this.hubConnection.on(method, (...args: unknown[]) =>
      subject.next(project(...(args as TArgs))),
    );
    return subject.asObservable();
  }

  private enqueue(operation: () => Promise<void>): Promise<void> {
    this.lifecycle = this.lifecycle.then(operation);
    return this.lifecycle;
  }

  /** No-ops while disconnected - {@link rejoinGroups} replays the membership once the hub is up. */
  private async invokeGroup(method: string, args: unknown[]): Promise<void> {
    if (!this.isConnected) {
      return;
    }

    try {
      await this.hubConnection.invoke(method, ...args);
    } catch (error) {
      console.error(`Failed to invoke ${method} on the ${this.hubName} hub`, error);
    }
  }

  private async rejoinGroups(): Promise<void> {
    for (const { method, args } of this.groupJoins.values()) {
      await this.invokeGroup(method, args);
    }
  }

  private async start(): Promise<void> {
    if (this.hubConnection.state !== HubConnectionState.Disconnected) {
      return;
    }

    this.state.set("connecting");
    try {
      await this.hubConnection.start();
      this.state.set("connected");
      await this.rejoinGroups();
    } catch (error) {
      this.state.set("disconnected");
      console.error(`Failed to connect to the ${this.hubName} hub`, error);
    }
  }

  private async stop(): Promise<void> {
    if (this.hubConnection.state !== HubConnectionState.Connected) {
      return;
    }

    try {
      // Stopping drops every membership server-side; a later start rebuilds what is still wanted.
      this.groupJoins.clear();
      await this.hubConnection.stop();
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
