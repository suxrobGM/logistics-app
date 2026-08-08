import type {
  AgentConversationDto,
  AgentConversationDtoPagedResponse,
  AgentSessionStatus,
  AIQuotaStatusDto,
} from "@logistics/shared/api";

/**
 * The HTTP surface every agent chat shares, which is what lets `withAgentChat` drive either
 * surface. `AgentChatApiBase` implements it for both.
 */
export interface AgentChatApi {
  fetchConversation(
    conversationId: string,
    options?: { silent?: boolean },
  ): Promise<AgentConversationDto | null>;
  fetchHistoryPage(
    page: number,
    pageSize: number,
  ): Promise<AgentConversationDtoPagedResponse | null>;
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
  userMessageSequence?: number;
}

/**
 * Progress of an in-flight turn, pushed over SignalR. Hand-written because SignalR payloads are not
 * in the OpenAPI spec; mirrors the backend's AgentTurnUpdateDto.
 */
export interface AgentTurnUpdate {
  conversationId: string;
  sessionId: string;
  status: AgentSessionStatus;
  totalTokensUsed: number;
  decisionCount: number;
  errorMessage: string | null;
}
