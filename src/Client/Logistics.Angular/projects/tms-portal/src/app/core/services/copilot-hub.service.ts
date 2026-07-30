import { Injectable } from "@angular/core";
import { getAccessToken } from "@logistics/shared";
import type {
  AgentDecisionDto,
  AgentSessionStatus,
  AICopilotMessageDto,
} from "@logistics/shared/api";
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
 * The setters are single-subscriber (`.off()` then `.on()`), so CopilotStore must stay the only
 * subscriber - components read the store, never this service.
 */
@Injectable({ providedIn: "root" })
export class CopilotHubService extends BaseHubConnection {
  constructor() {
    super("copilot", {
      accessTokenFactory: () => getAccessToken("tmsportal") ?? "",
      registerTenant: false,
    });
  }

  set onReceiveCopilotMessage(callback: (message: AICopilotMessageDto) => void) {
    this.hubConnection.off("ReceiveCopilotMessage");
    this.hubConnection.on("ReceiveCopilotMessage", callback);
  }

  set onReceiveCopilotDecision(callback: (decision: AgentDecisionDto) => void) {
    this.hubConnection.off("ReceiveCopilotDecision");
    this.hubConnection.on("ReceiveCopilotDecision", callback);
  }

  set onReceiveCopilotTurnUpdate(callback: (update: CopilotTurnUpdate) => void) {
    this.hubConnection.off("ReceiveCopilotTurnUpdate");
    this.hubConnection.on("ReceiveCopilotTurnUpdate", callback);
  }
}
