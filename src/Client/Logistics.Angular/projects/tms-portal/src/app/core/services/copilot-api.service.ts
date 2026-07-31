import { inject, Injectable } from "@angular/core";
import {
  Api,
  cancelCopilotTurn,
  createCopilotConversation,
  deleteCopilotConversation,
  getCopilotConversationById,
  getCopilotConversations,
  getCopilotQuotaStatus,
  renameCopilotConversation,
  sendCopilotMessage,
  silentErrors,
  type AICopilotConversationDto,
  type AICopilotConversationDtoPagedResult,
  type AIQuotaStatusDto,
  type SendAICopilotMessageResultDto,
} from "@logistics/shared/api";

/**
 * HTTP for the copilot drawer: every call resolves to null (or false for the commands whose only
 * output is success) instead of throwing, so CopilotStore stays pure state orchestration and never
 * touches try/catch.
 *
 * Failure toasts and upgrade prompts are NOT raised here - the global errorHandlerInterceptor
 * already does both for every request. Toasting again would show two toasts per failure.
 */
@Injectable({ providedIn: "root" })
export class CopilotApiService {
  private readonly api = inject(Api);

  /** `silent` for reconcile polls - a transient failure must not toast every 45s. */
  fetchConversation(
    conversationId: string,
    options?: { silent?: boolean },
  ): Promise<AICopilotConversationDto | null> {
    return this.orNull(
      this.api.invoke(
        getCopilotConversationById,
        { conversationId },
        options?.silent ? silentErrors() : undefined,
      ),
    );
  }

  fetchHistoryPage(
    page: number,
    pageSize: number,
  ): Promise<AICopilotConversationDtoPagedResult | null> {
    return this.orNull(
      this.api.invoke(getCopilotConversations, { Page: page, PageSize: pageSize }),
    );
  }

  /** Silent - the quota line is advisory; the send path enforces the limit server-side. */
  fetchQuota(): Promise<AIQuotaStatusDto | null> {
    return this.orNull(this.api.invoke(getCopilotQuotaStatus, undefined, silentErrors()));
  }

  createConversation(): Promise<AICopilotConversationDto | null> {
    return this.orNull(this.api.invoke(createCopilotConversation));
  }

  sendMessage(
    conversationId: string,
    text: string,
    pageContext: string,
  ): Promise<SendAICopilotMessageResultDto | null> {
    return this.orNull(
      this.api.invoke(sendCopilotMessage, {
        conversationId,
        body: { conversationId, text, pageContext },
      }),
    );
  }

  cancelTurn(conversationId: string): Promise<boolean> {
    return this.succeeded(this.api.invoke(cancelCopilotTurn, { conversationId }));
  }

  renameConversation(conversationId: string, title: string): Promise<boolean> {
    return this.succeeded(
      this.api.invoke(renameCopilotConversation, {
        conversationId,
        body: { conversationId, title },
      }),
    );
  }

  deleteConversation(conversationId: string): Promise<boolean> {
    return this.succeeded(this.api.invoke(deleteCopilotConversation, { conversationId }));
  }

  private async orNull<T>(call: Promise<T>): Promise<T | null> {
    try {
      return await call;
    } catch {
      return null;
    }
  }

  private async succeeded(call: Promise<unknown>): Promise<boolean> {
    try {
      await call;
      return true;
    } catch {
      return false;
    }
  }
}
