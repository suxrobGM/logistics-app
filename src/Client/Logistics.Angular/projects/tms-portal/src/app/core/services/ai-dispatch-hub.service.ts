import { Injectable } from "@angular/core";
import type { AIDispatchDecisionDto } from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

export interface AIDispatchUpdate {
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
export class AIDispatchHubService extends BaseHubConnection {
  constructor() {
    super("ai-dispatch");
  }

  set onReceiveAIDispatchUpdate(callback: (update: AIDispatchUpdate) => void) {
    this.hubConnection.off("ReceiveAIDispatchUpdate");
    this.hubConnection.on("ReceiveAIDispatchUpdate", callback);
  }

  set onReceiveAIDispatchDecision(callback: (decision: AIDispatchDecisionDto) => void) {
    this.hubConnection.off("ReceiveAIDispatchDecision");
    this.hubConnection.on("ReceiveAIDispatchDecision", callback);
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
