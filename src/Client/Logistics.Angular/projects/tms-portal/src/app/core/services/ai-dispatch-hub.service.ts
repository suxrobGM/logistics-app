import { DestroyRef, Injectable } from "@angular/core";
import type { AgentDecisionDto, AgentMessageDto, AgentSessionStatus } from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

/** Mirror of the backend AIDispatchTurnUpdateDto (SignalR payloads are not in the OpenAPI spec). */
export interface DispatchTurnUpdate {
  conversationId: string;
  sessionId: string;
  status: AgentSessionStatus;
  totalTokensUsed: number;
  decisionCount: number;
  errorMessage: string | null;
}

const DispatchBoardGroup = "dispatch-board";

/**
 * Real-time AI dispatch events. Unlike the copilot hub, every event goes to the whole tenant's
 * dispatch board group - one dispatcher's approval is reflected for everyone.
 */
@Injectable({ providedIn: "root" })
export class AIDispatchHubService extends BaseHubConnection {
  readonly messageReceived$ = this.event<AgentMessageDto>("ReceiveDispatchMessage");
  readonly turnUpdateReceived$ = this.event<DispatchTurnUpdate>("ReceiveDispatchTurnUpdate");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveAIDispatchDecision");

  constructor() {
    super("ai-dispatch");
  }

  /** Claims the connection and joins the tenant's board group for as long as `destroyRef` lives. */
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
