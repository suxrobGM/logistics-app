import { DestroyRef, inject, signal } from "@angular/core";
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
   * Whether to run the RegisterTenant/UnregisterTenant handshake. Authorized hubs derive identity
   * from JWT claims and skip it. Default true.
   */
  registerTenant?: boolean;
}

/** How long a hub lingers after its last consumer releases, so route changes don't churn it. */
const DisconnectLingerMs = 5000;

/**
 * Hub subclasses are root singletons shared by every consumer, so no single component may own the
 * connection lifecycle. Consumers call {@link acquire}, handing over their `DestroyRef` so the claim
 * dies with them; the connection stops once the last claim is gone. App-lifetime consumers acquire
 * without a `DestroyRef` and simply never release. There is deliberately no public disconnect - it
 * would kill the hub for every other consumer.
 *
 * Subclasses declare each server event with {@link event} / {@link mappedEvent} as a field
 * initializer, which registers the handler once and hands consumers an observable. A settable
 * callback would let one consumer steal or stack another's subscription - that bug class is why
 * this contract exists.
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

  /** Live connection status for offline/reconnecting UI. */
  readonly connectionState = this.state.asReadonly();

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
    this.hubConnection.onreconnected(() => this.state.set("connected"));
    this.hubConnection.onclose(() => this.state.set("disconnected"));
  }

  get isConnected(): boolean {
    return this.hubConnection.state === HubConnectionState.Connected;
  }

  /**
   * Claims the connection and ensures it is started. Pass the consumer's `DestroyRef` so the claim
   * is released with it; omit it only for app-lifetime consumers that never release.
   */
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
   * Releases a claim taken by {@link acquire} - unnecessary if you passed a `DestroyRef`. When the
   * last claim goes, the connection stops after a short linger (cancelled if someone re-acquires,
   * e.g. on a route change).
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

  /** Registers a server event once and exposes its single payload as an observable. */
  protected event<T>(method: string): Observable<T> {
    return this.mappedEvent(method, (payload: T) => payload);
  }

  /** {@link event} for hub methods that push several arguments, projected into one payload. */
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
        await this.hubConnection.invoke("RegisterTenant", tenant.id);
      }
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

      await this.hubConnection.stop();
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
