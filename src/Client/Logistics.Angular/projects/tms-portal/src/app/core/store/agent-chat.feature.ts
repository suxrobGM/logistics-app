import { computed, inject, type Type } from "@angular/core";
import type {
  AgentConversationDto,
  AgentDecisionDto,
  AgentMessageDto,
  AIQuotaStatusDto,
} from "@logistics/shared/api";
import { LocalizationService } from "@logistics/shared/services";
import {
  patchState,
  signalStoreFeature,
  withComputed,
  withMethods,
  withState,
  type WritableStateSource,
} from "@ngrx/signals";
import { AuthService } from "@/core/auth";
import type { AgentChatApi, AgentTurnUpdate } from "@/core/services/agent-chat.contracts";
import { buildQuotaNotice, TurnWatchdog } from "./agent-chat.helpers";

export type TurnStatus = "idle" | "running" | "failed";

/** The conversation/turn state every agent chat keeps, whatever chrome wraps it. */
export interface AgentChatState {
  conversations: AgentConversationDto[];
  currentConversation: AgentConversationDto | null;
  messages: AgentMessageDto[];
  decisions: AgentDecisionDto[];
  turnStatus: TurnStatus;
  turnError: string | null;
  /** Tools run so far in the active turn (from the per-iteration hub updates). */
  turnProgress: number | null;
  /** True once the active turn has run noticeably longer than usual. */
  longRunning: boolean;
  loading: boolean;
  sending: boolean;
  historyPage: number;
  hasMoreHistory: boolean;
  quota: AIQuotaStatusDto | null;
}

const initialAgentChatState: AgentChatState = {
  conversations: [],
  currentConversation: null,
  messages: [],
  decisions: [],
  turnStatus: "idle",
  turnError: null,
  turnProgress: null,
  longRunning: false,
  loading: false,
  sending: false,
  historyPage: 1,
  hasMoreHistory: false,
  quota: null,
};

export interface AgentChatConfig<Extra extends object> {
  /** Injected inside the feature, so each surface supplies its own endpoint set. */
  api: Type<AgentChatApi>;
  historyPageSize: number;
  /**
   * Surface state derived from the open conversation - dispatch's per-turn sessions, say. Called
   * with the loaded conversation, and with null when the chat resets, so both directions are one
   * definition rather than a hook per transition.
   */
  conversationExtras?: (conversation: AgentConversationDto | null) => Partial<Extra>;
}

/**
 * The conversation lifecycle both agent chats run: history paging, lazy conversation creation,
 * optimistic send, the turn state machine, and the watchdog that recovers a turn whose terminal hub
 * event never arrived.
 *
 * What it deliberately does NOT own is the hub subscriptions. Each surface's events carry different
 * side state (the drawer's unread count, the board's tenant-wide pending decisions), so the handlers
 * stay in the stores and call `appendMessage` / `upsertDecision` / `applyTurnUpdate` here.
 *
 * `Extra` is the host store's own state. Declare it with `withState` BEFORE this feature and pass
 * it as the type argument, so {@link AgentChatConfig.conversationExtras} patches keys that exist.
 * The feature is deliberately not typed over `Extra` beyond that callback: an unresolved
 * intersection would erase its own slice's types everywhere inside.
 */
export function withAgentChat<Extra extends object>(config: AgentChatConfig<Extra>) {
  return signalStoreFeature(
    withState<AgentChatState>(initialAgentChatState),

    withComputed((store, localization = inject(LocalizationService)) => ({
      isRunning: computed(() => store.turnStatus() === "running"),
      quotaNotice: computed(() =>
        buildQuotaNotice(store.quota(), (value) => localization.formatCurrency(value)),
      ),
      /** Owner opted for a hard pause and the budget is spent - the composer disables. */
      quotaBlocked: computed(() => store.quota()?.overageBlocked === true),
    })),

    withMethods((store, api = inject(config.api), auth = inject(AuthService)) => {
      // The host owns these keys, so they are outside this feature's state type by construction.
      const patchExtras = (conversation: AgentConversationDto | null): void => {
        const extras = config.conversationExtras?.(conversation);
        if (extras) patchState(store as WritableStateSource<object>, extras);
      };

      const beginTurn = (): void => {
        patchState(store, { turnStatus: "running", turnError: null, longRunning: false });
        watchdog.start();
      };

      const endTurn = (status: Exclude<TurnStatus, "running">, error?: string | null): void => {
        watchdog.stop();
        patchState(store, {
          turnStatus: status,
          turnError: error ?? null,
          turnProgress: null,
          longRunning: false,
        });
      };

      const applyConversation = (conversation: AgentConversationDto): void => {
        const inHistory = store.conversations().some((c) => c.id === conversation.id);
        patchState(store, {
          currentConversation: conversation,
          messages: conversation.messages ?? [],
          decisions: conversation.decisions ?? [],
          // Keeps the history row's title/last-message-time in step once the backend fills them in.
          conversations: inHistory
            ? store.conversations().map((c) => (c.id === conversation.id ? conversation : c))
            : store.conversations(),
        });
        patchExtras(conversation);
        // Guarded: re-running beginTurn mid-turn would reset the longRunning marker.
        if (conversation.status === "running") {
          if (store.turnStatus() !== "running") beginTurn();
        } else if (store.turnStatus() === "running") {
          endTurn("idle");
        }
      };

      /** Re-fetches the open conversation to recover state the hub failed to deliver. */
      const reconcile = async (): Promise<void> => {
        const conversationId = store.currentConversation()?.id;
        if (!conversationId) return;
        const conversation = await api.fetchConversation(conversationId, { silent: true });
        if (!conversation || store.currentConversation()?.id !== conversationId) return;
        applyConversation(conversation);
      };

      const watchdog = new TurnWatchdog(
        () => void reconcile(),
        () => patchState(store, { longRunning: true }),
      );

      const loadQuota = async (): Promise<void> => {
        const quota = await api.fetchQuota();
        if (quota) patchState(store, { quota });
      };

      const loadHistoryPage = async (page: number): Promise<void> => {
        const result = await api.fetchHistoryPage(page, config.historyPageSize);
        if (!result) return;
        const items = result.items ?? [];
        patchState(store, {
          conversations: page === 1 ? items : [...store.conversations(), ...items],
          historyPage: page,
          hasMoreHistory: page < (result.pagination?.totalPages ?? page),
        });
      };

      /** Conversations are created lazily, on the first send rather than on open. */
      const currentOrNewConversation = async (): Promise<AgentConversationDto | null> => {
        const existing = store.currentConversation();
        if (existing) return existing;

        const created = await api.createConversation();
        if (created) patchState(store, { currentConversation: created });
        return created;
      };

      const resetConversation = (): void => {
        // The conversation is created lazily on first send - no empty rows.
        endTurn("idle");
        patchState(store, { currentConversation: null, messages: [], decisions: [] });
        patchExtras(null);
      };

      const sendMessage = async (text: string): Promise<void> => {
        const trimmed = text.trim();
        if (!trimmed || store.sending() || store.turnStatus() === "running") return;

        patchState(store, { sending: true });
        try {
          const conversation = await currentOrNewConversation();
          if (!conversation) return;

          if (!store.conversations().some((c) => c.id === conversation.id)) {
            patchState(store, { conversations: [conversation, ...store.conversations()] });
          }

          // Deliberately untimestamped: it sorts last until the server's clock arrives, which is
          // where a just-sent message belongs anyway. A browser clock here would lose to the
          // server's on the reply and park the message below its own answer.
          const optimistic: AgentMessageDto = {
            id: `optimistic-${conversation.id}-${store.messages().length}`,
            conversationId: conversation.id,
            role: "user",
            text: trimmed,
            sentByUserId: auth.userId() ?? undefined,
          };
          patchState(store, { messages: [...store.messages(), optimistic] });
          beginTurn();

          const result = await api.sendMessage(conversation.id!, trimmed);
          if (!result) {
            patchState(store, {
              messages: store.messages().filter((m) => m.id !== optimistic.id),
            });
            endTurn("idle");
            return;
          }

          patchState(store, {
            messages: store.messages().map((m) =>
              m.id === optimistic.id
                ? {
                    ...m,
                    id: result.userMessageId,
                    createdAt: result.userMessageCreatedAt,
                    sequence: result.userMessageSequence,
                  }
                : m,
            ),
          });
        } finally {
          patchState(store, { sending: false });
        }
      };

      return {
        beginTurn,
        endTurn,
        applyConversation,
        resetConversation,
        reconcile,
        loadQuota,
        sendMessage,

        loadConversations: (): Promise<void> => loadHistoryPage(1),

        loadMoreConversations: (): Promise<void> => loadHistoryPage(store.historyPage() + 1),

        /** Loads a conversation into the chat; false means it could not be fetched. */
        async loadConversation(conversationId: string): Promise<boolean> {
          patchState(store, { loading: true });
          const conversation = await api.fetchConversation(conversationId);
          if (conversation) {
            patchState(store, { turnError: null, turnProgress: null });
            applyConversation(conversation);
          }
          patchState(store, { loading: false });
          return conversation !== null;
        },

        appendMessage(message: AgentMessageDto): void {
          const existing = store.messages();
          patchState(store, {
            messages: existing.some((m) => m.id === message.id)
              ? existing.map((m) => (m.id === message.id ? message : m))
              : [...existing, message],
          });
        },

        upsertDecision(decision: AgentDecisionDto): void {
          const existing = store.decisions();
          patchState(store, {
            decisions: existing.some((d) => d.id === decision.id)
              ? existing.map((d) => (d.id === decision.id ? decision : d))
              : [...existing, decision],
          });
        },

        applyTurnUpdate(update: AgentTurnUpdate): void {
          if (update.status === "running") {
            if (store.turnStatus() !== "running") {
              beginTurn();
            }
            patchState(store, { turnProgress: update.decisionCount, turnError: null });
            return;
          }

          endTurn(update.status === "failed" ? "failed" : "idle", update.errorMessage);
          void loadQuota();
        },

        /** Resends the last user message after a failed turn. */
        async retryTurn(): Promise<void> {
          if (store.sending() || store.turnStatus() === "running") return;
          const lastUserText = store
            .messages()
            .filter((m) => m.role === "user" && m.text)
            .at(-1)?.text;
          if (!lastUserText) return;
          endTurn("idle");
          await sendMessage(lastUserText);
        },

        async cancelTurn(): Promise<void> {
          const conversationId = store.currentConversation()?.id;
          if (!conversationId) return;
          if (await api.cancelTurn(conversationId)) {
            endTurn("idle");
          }
        },

        async renameConversation(conversationId: string, title: string): Promise<void> {
          const trimmed = title.trim();
          if (!trimmed || !(await api.renameConversation(conversationId, trimmed))) return;

          const current = store.currentConversation();
          patchState(store, {
            conversations: store
              .conversations()
              .map((c) => (c.id === conversationId ? { ...c, title: trimmed } : c)),
            currentConversation:
              current?.id === conversationId ? { ...current, title: trimmed } : current,
          });
        },

        async deleteConversation(conversationId: string): Promise<void> {
          if (!(await api.deleteConversation(conversationId))) return;

          patchState(store, {
            conversations: store.conversations().filter((c) => c.id !== conversationId),
          });
          if (store.currentConversation()?.id === conversationId) {
            resetConversation();
          }
        },
      };
    }),
  );
}
