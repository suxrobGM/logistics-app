import type { HttpClient, HttpContext } from "@angular/common/http";
import { inject } from "@angular/core";
import {
  Api,
  silentErrors,
  type AgentConversationDto,
  type AgentConversationDtoPagedResponse,
  type AIQuotaStatusDto,
  type StrictHttpResponse,
} from "@logistics/shared/api";
import type { Observable } from "rxjs";
import { orNull, succeeded } from "./agent-api.utils";
import type { AgentChatApi, SentAgentMessage } from "./agent-chat.contracts";

/**
 * One ng-openapi-gen operation constant. The generated barrel exports the functions, not this.
 * Spelled out rather than derived from a generated operation on purpose: a derived type would
 * always match whatever the generator emits, so a change in calling convention would slip through
 * the endpoint maps below instead of failing here.
 */
type AgentChatApiFn<TParams, TResult> = (
  http: HttpClient,
  rootUrl: string,
  params: TParams,
  context?: HttpContext,
) => Observable<StrictHttpResponse<TResult>>;

type NoParams = Record<string, never>;

interface ConversationParams {
  conversationId: string;
}

/** Which generated operations one agent chat surface's conversations live behind. */
export interface AgentChatEndpoints {
  getConversationById: AgentChatApiFn<ConversationParams, AgentConversationDto>;
  getConversations: AgentChatApiFn<
    { Page?: number; PageSize?: number; OrderBy?: string },
    AgentConversationDtoPagedResponse
  >;
  getQuotaStatus: AgentChatApiFn<NoParams, AIQuotaStatusDto>;
  createConversation: AgentChatApiFn<NoParams, AgentConversationDto>;
  sendMessage: AgentChatApiFn<
    { conversationId: string; body?: { conversationId?: string; text?: string | null } },
    SentAgentMessage
  >;
  cancelTurn: AgentChatApiFn<ConversationParams, void>;
  renameConversation: AgentChatApiFn<
    { conversationId: string; body?: { conversationId?: string; title?: string | null } },
    void
  >;
  deleteConversation: AgentChatApiFn<ConversationParams, void>;
}

/**
 * The conversation half of {@link AgentChatApi}, identical for every agent surface once its
 * endpoints are named. Subclasses supply {@link endpoints} and add whatever else their surface
 * needs.
 *
 * Every call resolves to null (or false where success is the only output) instead of throwing, so
 * the stores stay pure state orchestration. Failure toasts and upgrade prompts are NOT raised here -
 * the global errorHandlerInterceptor already does both, and toasting again would double up.
 */
export abstract class AgentChatApiBase implements AgentChatApi {
  protected readonly api = inject(Api);

  protected abstract readonly endpoints: AgentChatEndpoints;

  /** `silent` for reconcile polls - a transient failure must not toast every 45s. */
  fetchConversation(
    conversationId: string,
    options?: { silent?: boolean },
  ): Promise<AgentConversationDto | null> {
    return orNull(
      this.api.invoke(
        this.endpoints.getConversationById,
        { conversationId },
        options?.silent ? silentErrors() : undefined,
      ),
    );
  }

  fetchHistoryPage(
    page: number,
    pageSize: number,
  ): Promise<AgentConversationDtoPagedResponse | null> {
    return orNull(
      this.api.invoke(this.endpoints.getConversations, { Page: page, PageSize: pageSize }),
    );
  }

  /** Silent - the quota notice and composer block are advisory; the send path enforces server-side. */
  fetchQuota(): Promise<AIQuotaStatusDto | null> {
    return orNull(this.api.invoke(this.endpoints.getQuotaStatus, {}, silentErrors()));
  }

  createConversation(): Promise<AgentConversationDto | null> {
    return orNull(this.api.invoke(this.endpoints.createConversation, {}));
  }

  sendMessage(conversationId: string, text: string): Promise<SentAgentMessage | null> {
    return orNull(
      this.api.invoke(this.endpoints.sendMessage, {
        conversationId,
        body: { conversationId, text },
      }),
    );
  }

  cancelTurn(conversationId: string): Promise<boolean> {
    return succeeded(this.api.invoke(this.endpoints.cancelTurn, { conversationId }));
  }

  renameConversation(conversationId: string, title: string): Promise<boolean> {
    return succeeded(
      this.api.invoke(this.endpoints.renameConversation, {
        conversationId,
        body: { conversationId, title },
      }),
    );
  }

  deleteConversation(conversationId: string): Promise<boolean> {
    return succeeded(this.api.invoke(this.endpoints.deleteConversation, { conversationId }));
  }
}
