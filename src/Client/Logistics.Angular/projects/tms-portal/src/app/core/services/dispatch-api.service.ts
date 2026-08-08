import { inject, Injectable } from "@angular/core";
import {
  Api,
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
  type AgentConversationDto,
  type AgentConversationDtoPagedResult,
  type AgentDecisionDto,
  type AIQuotaStatusDto,
  type SendAIDispatchMessageResultDto,
  type TruckDto,
} from "@logistics/shared/api";
import { orNull, succeeded } from "./agent-api.utils";

/**
 * HTTP for the AI dispatch chat page. Calls resolve to null/false instead of throwing (mirrors
 * CopilotApiService), and never toast - the global errorHandlerInterceptor already does.
 */
@Injectable({ providedIn: "root" })
export class DispatchApiService {
  private readonly api = inject(Api);

  /** `silent` for reconcile polls - a transient failure must not toast every 45s. */
  fetchConversation(
    conversationId: string,
    options?: { silent?: boolean },
  ): Promise<AgentConversationDto | null> {
    return orNull(
      this.api.invoke(
        getAIDispatchConversationById,
        { conversationId },
        options?.silent ? silentErrors() : undefined,
      ),
    );
  }

  fetchHistoryPage(
    page: number,
    pageSize: number,
  ): Promise<AgentConversationDtoPagedResult | null> {
    return orNull(
      this.api.invoke(getAIDispatchConversations, {
        Page: page,
        PageSize: pageSize,
        OrderBy: "-LastMessageAt",
      }),
    );
  }

  /** Silent - the quota notice and composer block are advisory; the send path enforces server-side. */
  fetchQuota(): Promise<AIQuotaStatusDto | null> {
    return orNull(this.api.invoke(getAIQuotaStatus, undefined, silentErrors()));
  }

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

  createConversation(): Promise<AgentConversationDto | null> {
    return orNull(this.api.invoke(createAIDispatchConversation));
  }

  sendMessage(
    conversationId: string,
    text: string,
  ): Promise<SendAIDispatchMessageResultDto | null> {
    return orNull(
      this.api.invoke(sendAIDispatchMessage, {
        conversationId,
        body: { conversationId, text },
      }),
    );
  }

  cancelTurn(conversationId: string): Promise<boolean> {
    return succeeded(this.api.invoke(cancelAIDispatchTurn, { conversationId }));
  }

  renameConversation(conversationId: string, title: string): Promise<boolean> {
    return succeeded(
      this.api.invoke(renameAIDispatchConversation, {
        conversationId,
        body: { conversationId, title },
      }),
    );
  }

  deleteConversation(conversationId: string): Promise<boolean> {
    return succeeded(this.api.invoke(deleteAIDispatchConversation, { conversationId }));
  }
}
