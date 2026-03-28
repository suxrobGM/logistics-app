import { Injectable } from "@angular/core";
import type { DispatchDecisionDto } from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

export interface DispatchAgentUpdate {
  sessionId: string;
  status: string;
  mode: string;
  decisionCount: number;
  summary: string | null;
}

/**
 * Service for managing real-time AI dispatch agent operations via SignalR.
 */
@Injectable({ providedIn: "root" })
export class DispatchAgentHubService extends BaseHubConnection {
  constructor() {
    super("dispatch-agent");
  }

  set onReceiveDispatchAgentUpdate(callback: (update: DispatchAgentUpdate) => void) {
    this.hubConnection.off("ReceiveDispatchAgentUpdate");
    this.hubConnection.on("ReceiveDispatchAgentUpdate", callback);
  }

  set onReceiveDispatchDecision(callback: (decision: DispatchDecisionDto) => void) {
    this.hubConnection.off("ReceiveDispatchDecision");
    this.hubConnection.on("ReceiveDispatchDecision", callback);
  }

  async subscribeToDispatchBoard(tenantId: string): Promise<void> {
    if (!this.isConnected) {
      return;
    }
    try {
      await this.hubConnection.invoke("SubscribeToDispatchBoard", tenantId);
    } catch (error) {
      console.error("Failed to subscribe to dispatch board", error);
    }
  }

  async unsubscribeFromDispatchBoard(tenantId: string): Promise<void> {
    if (!this.isConnected) {
      return;
    }
    try {
      await this.hubConnection.invoke("UnsubscribeFromDispatchBoard", tenantId);
    } catch (error) {
      console.error("Failed to unsubscribe from dispatch board", error);
    }
  }
}
