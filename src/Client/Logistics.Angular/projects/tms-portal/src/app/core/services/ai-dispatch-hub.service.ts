import { DestroyRef, Injectable } from "@angular/core";
import type { AgentDecisionDto, AgentMessageDto, RateNegotiationDto } from "@logistics/shared/api";
import type { AgentTurnUpdate } from "./agent-chat.contracts";
import { BaseHubConnection } from "./base-hub-connection";

/**
 * Real-time AI dispatch events. Unlike the copilot hub, every event goes to the whole tenant's
 * dispatch board group - one dispatcher's approval is reflected for everyone.
 */
@Injectable({ providedIn: "root" })
export class AIDispatchHubService extends BaseHubConnection {
  readonly messageReceived$ = this.event<AgentMessageDto>("ReceiveDispatchMessage");
  readonly turnUpdateReceived$ = this.event<AgentTurnUpdate>("ReceiveDispatchTurnUpdate");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveAIDispatchDecision");
  readonly negotiationReceived$ = this.event<RateNegotiationDto>("ReceiveNegotiationUpdate");

  constructor() {
    super("ai-dispatch");
  }

  /**
   * Claims the connection for as long as `destroyRef` lives. The server joins the board group
   * from the JWT tenant claim on connect, so there is no group call to make here.
   */
  acquireDispatchBoard(destroyRef: DestroyRef): Promise<void> {
    return this.acquire(destroyRef);
  }
}
