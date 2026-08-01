import { DestroyRef, Injectable } from "@angular/core";
import type { AgentDecisionDto } from "@logistics/shared/api";
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
  readonly updateReceived$ = this.event<AIDispatchUpdate>("ReceiveAIDispatchUpdate");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveAIDispatchDecision");

  constructor() {
    super("ai-dispatch");
  }

  /**
   * Claims the connection and joins the tenant's dispatch board group, both for as long as
   * `destroyRef` lives. Board pages need nothing else to receive updates.
   */
  async acquireDispatchBoard(destroyRef: DestroyRef): Promise<void> {
    const tenantId = this.tenantService.getTenantData()?.id;
    if (!tenantId) {
      return;
    }

    await this.acquire(destroyRef);
    destroyRef.onDestroy(() => void this.unsubscribeFromDispatchBoard(tenantId));
    await this.subscribeToDispatchBoard(tenantId);
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
