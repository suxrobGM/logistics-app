import { computed, DestroyRef, signal } from "@angular/core";
import { getAccessToken } from "@logistics/shared";
import {
  HttpTransportType,
  HubConnection,
  HubConnectionBuilder,
  HubConnectionState,
} from "@microsoft/signalr";
import { Observable, Subject } from "rxjs";
import { environment } from "@/env";

export type HubConnectionStatus = "disconnected" | "connecting" | "connected" | "reconnecting";

/** How long a hub stays open after its last user leaves, so route changes don't churn it. */
const DisconnectDelayMs = 5000;

/** Shared SignalR connection with reference-counted lifecycle and reconnect-safe group membership. */
export abstract class BaseHubConnection {
  private readonly state = signal<HubConnectionStatus>("disconnected");
  protected readonly hubConnection: HubConnection;

  private activeUsers = 0;
  private disconnectTimer: ReturnType<typeof setTimeout> | null = null;
  /** Runs connect and disconnect one at a time, in the order they were asked for. */
  private pendingChange: Promise<void> = Promise.resolve();

  /** Group joins replayed after reconnecting with a new connection ID. */
  private readonly joinedGroups = new Map<string, { method: string; args: unknown[] }>();

  /** Live connection status for offline/reconnecting UI. */
  readonly connectionState = this.state.asReadonly();

  /** Whether live updates are currently unavailable. */
  readonly realtimeDown = computed(
    () => this.state() === "disconnected" || this.state() === "reconnecting",
  );

  constructor(private readonly hubName: string) {
    this.hubConnection = new HubConnectionBuilder()
      .withUrl(`${environment.apiUrl}/hubs/${hubName}`, {
        skipNegotiation: true,
        transport: HttpTransportType.WebSockets,
        accessTokenFactory: () => getAccessToken("tmsportal") ?? "",
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

  /** Opens the connection and keeps it open for this caller's lifetime. */
  connect(destroyRef?: DestroyRef): Promise<void> {
    if (this.disconnectTimer) {
      clearTimeout(this.disconnectTimer);
      this.disconnectTimer = null;
    }
    this.activeUsers++;
    destroyRef?.onDestroy(() => this.disconnect());
    return this.runInOrder(() => this.start());
  }

  /** Signals this caller is done. The connection closes once the last one leaves. */
  disconnect(): void {
    if (this.activeUsers > 0) {
      this.activeUsers--;
    }
    if (this.activeUsers > 0 || this.disconnectTimer) {
      return;
    }
    this.disconnectTimer = setTimeout(() => {
      this.disconnectTimer = null;
      void this.runInOrder(() => this.stop());
    }, DisconnectDelayMs);
  }

  /** Joins a group now and after each reconnect. */
  protected async joinGroup(key: string, method: string, ...args: unknown[]): Promise<void> {
    this.joinedGroups.set(key, { method, args });
    await this.callHub(method, args);
  }

  /** Leaves a {@link joinGroup} membership and stops replaying it. */
  protected async leaveGroup(key: string, method: string, ...args: unknown[]): Promise<void> {
    this.joinedGroups.delete(key);
    await this.callHub(method, args);
  }

  /** Registers a server event once and exposes its payload as an observable. */
  protected event<T>(method: string): Observable<T> {
    return this.eventWithArgs(method, (payload: T) => payload);
  }

  /** {@link event} for hub methods that send several arguments. */
  protected eventWithArgs<TArgs extends unknown[], T>(
    method: string,
    project: (...args: TArgs) => T,
  ): Observable<T> {
    const subject = new Subject<T>();
    this.hubConnection.on(method, (...args: unknown[]) =>
      subject.next(project(...(args as TArgs))),
    );
    return subject.asObservable();
  }

  private runInOrder(operation: () => Promise<void>): Promise<void> {
    this.pendingChange = this.pendingChange.then(operation);
    return this.pendingChange;
  }

  /** No-ops while disconnected - {@link rejoinGroups} replays the membership once the hub is up. */
  private async callHub(method: string, args: unknown[]): Promise<void> {
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
    for (const { method, args } of this.joinedGroups.values()) {
      await this.callHub(method, args);
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
      this.joinedGroups.clear();
      await this.hubConnection.stop();
    } catch (error) {
      console.error(`Failed to disconnect from the ${this.hubName} hub`, error);
    }
  }
}
