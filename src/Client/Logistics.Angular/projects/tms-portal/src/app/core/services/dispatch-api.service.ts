import { Injectable } from "@angular/core";
import {
  cancelAIDispatchTurn,
  createAIDispatchConversation,
  deleteAIDispatchConversation,
  getAIDispatchConversationById,
  getAIDispatchConversations,
  getAIQuotaStatus,
  getPendingDecisions,
  getTrucks,
  renameAIDispatchConversation,
  sendAIDispatchMessage,
  silentErrors,
  type AgentDecisionDto,
  type TruckDto,
} from "@logistics/shared/api";
import { orNull } from "./agent-api.utils";
import { AgentChatApiBase, type AgentChatEndpoints } from "./agent-chat-api.base";

/** HTTP for the AI dispatch chat page. */
@Injectable({ providedIn: "root" })
export class DispatchApiService extends AgentChatApiBase {
  protected readonly endpoints: AgentChatEndpoints = {
    getConversationById: getAIDispatchConversationById,
    getConversations: getAIDispatchConversations,
    getQuotaStatus: getAIQuotaStatus,
    createConversation: createAIDispatchConversation,
    sendMessage: sendAIDispatchMessage,
    cancelTurn: cancelAIDispatchTurn,
    renameConversation: renameAIDispatchConversation,
    deleteConversation: deleteAIDispatchConversation,
    historyOrderBy: "-LastMessageAt",
  };

  /** Tenant-wide write decisions awaiting approval, for the right panel + sidebar badge. */
  fetchPendingDecisions(options?: { silent?: boolean }): Promise<AgentDecisionDto[] | null> {
    return orNull(
      this.api.invoke(getPendingDecisions, undefined, options?.silent ? silentErrors() : undefined),
    );
  }

  /** Trucks with a known location, for the fleet map. Silent - the map is a secondary panel. */
  async fetchAvailableTrucks(): Promise<TruckDto[]> {
    const result = await orNull(
      this.api.invoke(getTrucks, { Statuses: ["available"], PageSize: 100 }, silentErrors()),
    );
    return result?.items ?? [];
  }
}
