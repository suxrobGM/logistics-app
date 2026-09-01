import { Injectable } from "@angular/core";
import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";
import type { AgentTurnUpdate } from "./agent-chat.contracts";
import { BaseHubConnection } from "./base-hub-connection";

/** Streams private copilot events to the authenticated user. */
@Injectable({ providedIn: "root" })
export class CopilotHubService extends BaseHubConnection {
  readonly messageReceived$ = this.event<AgentMessageDto>("ReceiveCopilotMessage");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveCopilotDecision");
  readonly turnUpdateReceived$ = this.event<AgentTurnUpdate>("ReceiveCopilotTurnUpdate");

  constructor() {
    super("copilot");
  }
}
