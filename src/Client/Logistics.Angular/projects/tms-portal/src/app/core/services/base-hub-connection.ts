import { computed, DestroyRef, inject, signal } from "@angular/core";
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
  /** JWT for authorized hubs; SignalR sends it as the access_token query parameter. */
  accessTokenFactory?: () => string | Promise<string>;

  /**
   * Runs the RegisterTenant/UnregisterTenant handshake. Authorized hubs take identity from JWT
   * claims and skip it. Default true.
   */
  registerTenant?: boolean;
}

/** How long a hub lingers after its last consumer releases, so route changes don't churn it. */
const DisconnectLingerMs = 5000;

/**
 * Hub subclasses are root singletons shared by every consumer, so no single component may own the
 * connection lifecycle. Consumers {@link acquire} with their `DestroyRef` and the hub stops once the
 * last claim is gone. There is no public disconnect - it would kill the hub for everyone else.
 *
 * Declare events with {@link event} / {@link mappedEvent} as field initializers and join groups
 * through {@link joinGroup}: a settable callback lets one consumer steal another's subscription,
 * and a raw group invoke is lost on reconnect (see {@link groupJoins}).
 */
export abstract class BaseHubConnection {
  protected readonly tenantService = inject(TenantService);
  private readonly registerTenant: boolean;
  private readonly state = signal<HubConnectionStatus>("disconnected");
  protected readonly hubConnection: HubConnection;

  private claims = 0;
  private lingerTimer: ReturnType<typeof setTimeout> | null = null;
  /** Serializes start/stop so a release-then-acquire (route change) cannot interleave. */
  private lifecycle: Promise<void> = Promise.resolve();

  /**
   * Replayed after every connect. SignalR keys groups by connection id and `withAutomaticReconnect`
   * hands out a new one, so a reconnect silently drops every membership - the socket comes back up
   * and no event ever arrives again.
   */
  private readonly groupJoins = new Map<string, { method: string; args: unknown[] }>();

  /** Live connection status for offline/reconnecting UI. */
  readonly connectionState = this.state.asReadonly();

  /**
   * Whether to tell the user live updates are gone. "connecting" is the first connect and is not
   * "down" - showing the banner there would flash it on every page load.
   */
  readonly realtimeDown = computed(
    () => this.state() === "disconnected" || this.state() === "reconnecting",
  );

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

  /** Claims the connection and starts it. Pass a `DestroyRef` unless the consumer is app-lifetime. */
  acquire(destroyRef?: DestroyRef): Promise<void> {
    if (this.lingerTimer) {
      clearTimeout(this.lingerTimer);
      this.lingerTimer = null;
    }
    this.claims++;
    destroyRef?.onDestroy(() => this.release());
    return this.enqueue(() => this.start());
  }

  /**
   * Unnecessary if {@link acquire} was given a `DestroyRef`. The last release stops the hub after a
   * linger, cancelled by a re-acquire (e.g. a route change).
   */
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

  /**
   * Joins now if connected, and again after every reconnect. `key` is local to the client: a later
   * join under the same key replaces it (one dispatch board, one open conversation).
   */
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

      if (this.registerTenant) {
        const tenant = this.tenantService.getTenantData();
        if (!tenant) {
          console.error(`Failed to connect to the ${this.hubName} hub, tenant ID is null`);
          return;
        }
        this.groupJoins.set("tenant", { method: "RegisterTenant", args: [tenant.id] });
      }

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
      if (this.registerTenant) {
        const tenant = this.tenantService.getTenantData();
        if (tenant) {
          await this.hubConnection.invoke("UnregisterTenant", tenant.id);
        }
      }

      // Stopping drops every membership server-side; a later start rebuilds what is still wanted.
      this.groupJoins.clear();
      await this.hubConnection.stop();
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
