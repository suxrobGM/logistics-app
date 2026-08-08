import { Injectable } from "@angular/core";
import {
  cancelCopilotTurn,
  createCopilotConversation,
  deleteCopilotConversation,
  getCopilotConversationById,
  getCopilotConversations,
  getCopilotQuotaStatus,
  renameCopilotConversation,
  sendCopilotMessage,
} from "@logistics/shared/api";
import { AgentChatApiBase, type AgentChatEndpoints } from "./agent-chat-api.base";

/** HTTP for the copilot drawer. */
@Injectable({ providedIn: "root" })
export class CopilotApiService extends AgentChatApiBase {
  protected readonly endpoints: AgentChatEndpoints = {
    getConversationById: getCopilotConversationById,
    getConversations: getCopilotConversations,
    getQuotaStatus: getCopilotQuotaStatus,
    createConversation: createCopilotConversation,
    sendMessage: sendCopilotMessage,
    cancelTurn: cancelCopilotTurn,
    renameConversation: renameCopilotConversation,
    deleteConversation: deleteCopilotConversation,
  };
}
