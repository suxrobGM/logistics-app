import { Injectable } from "@angular/core";
import type { DispatchDecisionDto } from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { BaseHubConnection } from "./base-hub-connection";

export interface DispatchAgentUpdate {
  sessionId: string;
  status: string;
  mode: string;
  decisionCount: number;
  summary: string | null;
}

/**
 * Service for managing real-time tracking of trucks and dispatch decisions via SignalR.
 */
@Injectable({ providedIn: "root" })
export class TrackingService extends BaseHubConnection {
  constructor() {
    super("tracking");
  }

  set onReceiveGeolocationData(callback: (geolocation: TruckGeolocationDto) => void) {
    this.hubConnection.on("ReceiveGeolocationData", callback);
  }

  set onReceiveDispatchAgentUpdate(callback: (update: DispatchAgentUpdate) => void) {
    this.hubConnection.on("ReceiveDispatchAgentUpdate", callback);
  }

  set onReceiveDispatchDecision(callback: (decision: DispatchDecisionDto) => void) {
    this.hubConnection.on("ReceiveDispatchDecision", callback);
  }

  async subscribeToDispatchBoard(tenantId: string): Promise<void> {
    try {
      await this.hubConnection.invoke("SubscribeToDispatchBoard", tenantId);
    } catch (error) {
      console.error("Failed to subscribe to dispatch board", error);
    }
  }

  async unsubscribeFromDispatchBoard(tenantId: string): Promise<void> {
    try {
      await this.hubConnection.invoke("UnsubscribeFromDispatchBoard", tenantId);
    } catch (error) {
      console.error("Failed to unsubscribe from dispatch board", error);
    }
  }
}
