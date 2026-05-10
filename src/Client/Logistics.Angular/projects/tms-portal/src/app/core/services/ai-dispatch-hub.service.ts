import { Injectable } from "@angular/core";
import type { AiDispatchDecisionDto } from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

export interface AiDispatchUpdate {
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
export class AiDispatchHubService extends BaseHubConnection {
  constructor() {
    super("ai-dispatch");
  }

  set onReceiveAiDispatchUpdate(callback: (update: AiDispatchUpdate) => void) {
    this.hubConnection.off("ReceiveAiDispatchUpdate");
    this.hubConnection.on("ReceiveAiDispatchUpdate", callback);
  }

  set onReceiveAiDispatchDecision(callback: (decision: AiDispatchDecisionDto) => void) {
    this.hubConnection.off("ReceiveAiDispatchDecision");
    this.hubConnection.on("ReceiveAiDispatchDecision", callback);
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
