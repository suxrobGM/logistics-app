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

const DispatchBoardGroup = "dispatch-board";

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

    destroyRef.onDestroy(
      () => void this.leaveGroup(DispatchBoardGroup, "UnsubscribeFromDispatchBoard", tenantId),
    );
    await this.acquire(destroyRef);
    await this.joinGroup(DispatchBoardGroup, "SubscribeToDispatchBoard", tenantId);
  }
}
