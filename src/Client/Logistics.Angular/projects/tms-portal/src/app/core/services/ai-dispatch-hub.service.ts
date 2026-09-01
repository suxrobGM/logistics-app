import { DestroyRef, Injectable } from "@angular/core";
import type { AgentDecisionDto, AgentMessageDto, RateNegotiationDto } from "@logistics/shared/api";
import type { AgentTurnUpdate } from "./agent-chat.contracts";
import { BaseHubConnection } from "./base-hub-connection";

/** Streams AI dispatch events to the tenant's dispatch board. */
@Injectable({ providedIn: "root" })
export class AIDispatchHubService extends BaseHubConnection {
  readonly messageReceived$ = this.event<AgentMessageDto>("ReceiveDispatchMessage");
  readonly turnUpdateReceived$ = this.event<AgentTurnUpdate>("ReceiveDispatchTurnUpdate");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveAIDispatchDecision");
  readonly negotiationReceived$ = this.event<RateNegotiationDto>("ReceiveNegotiationUpdate");

  constructor() {
    super("ai-dispatch");
  }

  /** Keeps the dispatch-board connection alive for the consumer's lifetime. */
  acquireDispatchBoard(destroyRef: DestroyRef): Promise<void> {
    return this.acquire(destroyRef);
  }
}
