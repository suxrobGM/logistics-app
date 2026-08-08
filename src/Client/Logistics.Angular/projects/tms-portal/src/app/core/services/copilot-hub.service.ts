import { Injectable } from "@angular/core";
import { getAccessToken } from "@logistics/shared";
import type { AgentDecisionDto, AgentMessageDto, AgentSessionStatus } from "@logistics/shared/api";
import { BaseHubConnection } from "./base-hub-connection";

/** Mirror of the backend AICopilotTurnUpdateDto (SignalR payloads are not in the OpenAPI spec). */
export interface CopilotTurnUpdate {
  conversationId: string;
  sessionId: string;
  status: AgentSessionStatus;
  totalTokensUsed: number;
  decisionCount: number;
  errorMessage: string | null;
}

/**
 * Real-time copilot events. The hub is authorized: identity comes from the JWT and the server
 * auto-joins the connection to its private per-user group - no RegisterTenant handshake.
 *
 * CopilotStore is the only intended subscriber - components read the store, never this service.
 */
@Injectable({ providedIn: "root" })
export class CopilotHubService extends BaseHubConnection {
  readonly messageReceived$ = this.event<AgentMessageDto>("ReceiveCopilotMessage");
  readonly decisionReceived$ = this.event<AgentDecisionDto>("ReceiveCopilotDecision");
  readonly turnUpdateReceived$ = this.event<CopilotTurnUpdate>("ReceiveCopilotTurnUpdate");

  constructor() {
    super("copilot", {
      accessTokenFactory: () => getAccessToken("tmsportal") ?? "",
      registerTenant: false,
    });
  }
}
