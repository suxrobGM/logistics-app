import type {
  AgentConversationDto,
  AgentConversationDtoPagedResult,
  AgentSessionStatus,
  AIQuotaStatusDto,
} from "@logistics/shared/api";

/**
 * The HTTP surface every agent chat shares. `CopilotApiService` and `DispatchApiService` implement
 * it structurally, which is what lets `withAgentChat` drive either one.
 *
 * Every call resolves to null (or false for the commands whose only output is success) rather than
 * throwing, so the stores stay pure state orchestration and never touch try/catch.
 */
export interface AgentChatApi {
  /** `silent` for reconcile polls - a transient failure must not toast every 45s. */
  fetchConversation(
    conversationId: string,
    options?: { silent?: boolean },
  ): Promise<AgentConversationDto | null>;
  fetchHistoryPage(page: number, pageSize: number): Promise<AgentConversationDtoPagedResult | null>;
  fetchQuota(): Promise<AIQuotaStatusDto | null>;
  createConversation(): Promise<AgentConversationDto | null>;
  sendMessage(conversationId: string, text: string): Promise<SentAgentMessage | null>;
  cancelTurn(conversationId: string): Promise<boolean>;
  renameConversation(conversationId: string, title: string): Promise<boolean>;
  deleteConversation(conversationId: string): Promise<boolean>;
}

/** The server's identity for a message the client already echoed optimistically. */
export interface SentAgentMessage {
  userMessageId?: string;
  userMessageCreatedAt?: string;
}

/**
 * Progress of an in-flight turn, pushed over SignalR. Mirrors the backend's
 * AICopilotTurnUpdateDto / AIDispatchTurnUpdateDto - identical shapes, and SignalR payloads are
 * not in the OpenAPI spec.
 */
export interface AgentTurnUpdate {
  conversationId: string;
  sessionId: string;
  status: AgentSessionStatus;
  totalTokensUsed: number;
  decisionCount: number;
  errorMessage: string | null;
}
