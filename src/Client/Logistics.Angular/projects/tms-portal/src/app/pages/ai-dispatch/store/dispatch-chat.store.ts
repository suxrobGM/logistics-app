import { computed, DestroyRef, effect, inject } from "@angular/core";
import type {
  AgentConversationDto,
  AgentDecisionDto,
  AgentMessageDto,
  AgentSessionDto,
  AIQuotaStatusDto,
  TruckDto,
} from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { LocalizationService } from "@logistics/shared/services";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { AIDispatchHubService } from "@/core/services/ai-dispatch-hub.service";
import { DispatchApiService } from "@/core/services/dispatch-api.service";
import { DispatchBadgeService } from "@/core/services/dispatch-badge.service";
import {
  buildQuotaNotice,
  persistRightPanelCollapsed,
  readStoredRightPanelCollapsed,
  TurnWatchdog,
} from "./dispatch-chat.store.helpers";

type TurnStatus = "idle" | "running" | "failed";

const HistoryPageSize = 30;

interface DispatchChatState {
  conversations: AgentConversationDto[];
  currentConversation: AgentConversationDto | null;
  messages: AgentMessageDto[];
  decisions: AgentDecisionDto[];
  sessions: AgentSessionDto[];
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
  /** Tenant-wide write decisions awaiting approval - the right panel + sidebar nav badge. */
  pendingDecisions: AgentDecisionDto[];
  trucks: TruckDto[];
  quota: AIQuotaStatusDto | null;
  sidebarCollapsed: boolean;
  rightPanelCollapsed: boolean;
}

const initialState: DispatchChatState = {
  conversations: [],
  currentConversation: null,
  messages: [],
  decisions: [],
  sessions: [],
  turnStatus: "idle",
  turnError: null,
  turnProgress: null,
  longRunning: false,
  loading: false,
  sending: false,
  historyPage: 1,
  hasMoreHistory: false,
  pendingDecisions: [],
  trucks: [],
  quota: null,
  sidebarCollapsed: false,
  rightPanelCollapsed: readStoredRightPanelCollapsed(),
};

/**
 * Page-scoped (provided by `DispatchChat`, not root) so the dispatch-board hub claim releases on
 * navigation - nothing outside the page needs this state. HTTP lives in `DispatchApiService`.
 */
export const DispatchChatStore = signalStore(
  withState(initialState),

  withComputed(
    (store, localization = inject(LocalizationService), hub = inject(AIDispatchHubService)) => ({
      isRunning: computed(() => store.turnStatus() === "running"),
      /** "connecting" is not "down" - the banner should only show for lost/failed connections. */
      realtimeDown: computed(() => {
        const state = hub.connectionState();
        return state === "disconnected" || state === "reconnecting";
      }),
      quotaNotice: computed(() =>
        buildQuotaNotice(store.quota(), (value) => localization.formatCurrency(value)),
      ),
      /** Owner opted for a hard pause and the budget is spent - the composer disables. */
      quotaBlocked: computed(() => store.quota()?.overageBlocked === true),
      /** Only write-tool decisions (assign, create trip, dispatch...) need approval - queries never do. */
      writeDecisions: computed(() => store.pendingDecisions().filter((d) => d.type !== "query")),
      truckLocations: computed<TruckGeolocationDto[]>(() =>
        store
          .trucks()
          .filter((t) => t.currentLocation?.latitude && t.currentLocation?.longitude)
          .map((t) => ({
            truckId: t.id,
            truckNumber: t.number,
            driversName: [t.mainDriver?.fullName, t.secondaryDriver?.fullName]
              .filter(Boolean)
              .join(", "),
            currentLocation: t.currentLocation,
            currentAddress: t.currentAddress,
          })),
      ),
    }),
  ),

  withMethods(
    (
      store,
      dispatchApi = inject(DispatchApiService),
      hub = inject(AIDispatchHubService),
      dispatchBadge = inject(DispatchBadgeService),
      destroyRef = inject(DestroyRef),
    ) => {
      const watchdog = new TurnWatchdog(
        () => void reconcileConversation(),
        () => patchState(store, { longRunning: true }),
      );

      effect(() => dispatchBadge.pendingCount.set(store.writeDecisions().length));

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
        const inSidebar = store.conversations().some((c) => c.id === conversation.id);
        patchState(store, {
          currentConversation: conversation,
          messages: conversation.messages ?? [],
          decisions: conversation.decisions ?? [],
          sessions: conversation.sessions ?? [],
          // Keeps the sidebar's title/last-message-time in step once the backend fills them in.
          conversations: inSidebar
            ? store.conversations().map((c) => (c.id === conversation.id ? conversation : c))
            : store.conversations(),
        });
        // Guarded: re-running beginTurn mid-turn would reset the longRunning marker.
        if (conversation.status === "running") {
          if (store.turnStatus() !== "running") beginTurn();
        } else if (store.turnStatus() === "running") {
          endTurn("idle");
        }
      };

      /** Conversations are created lazily, on the first send rather than on page load. */
      const currentOrNewConversation = async (): Promise<AgentConversationDto | null> => {
        const existing = store.currentConversation();
        if (existing) return existing;

        const created = await dispatchApi.createConversation();
        if (created) patchState(store, { currentConversation: created });
        return created;
      };

      /** Re-fetches the open conversation to recover state the hub failed to deliver. */
      const reconcileConversation = async (): Promise<void> => {
        const conversationId = store.currentConversation()?.id;
        if (!conversationId) return;
        const conversation = await dispatchApi.fetchConversation(conversationId, { silent: true });
        if (!conversation || store.currentConversation()?.id !== conversationId) return;
        applyConversation(conversation);
      };

      const loadQuota = async (): Promise<void> => {
        const quota = await dispatchApi.fetchQuota();
        if (quota) patchState(store, { quota });
      };

      const refreshPendingDecisions = async (): Promise<void> => {
        const pending = await dispatchApi.fetchPendingDecisions({ silent: true });
        if (pending) patchState(store, { pendingDecisions: pending });
      };

      const loadTrucks = async (): Promise<void> => {
        patchState(store, { trucks: await dispatchApi.fetchAvailableTrucks() });
      };

      // No takeUntilDestroyed: the store is page-scoped, so these die with the page.
      hub.messageReceived$.subscribe((message) => {
        if (message.conversationId !== store.currentConversation()?.id) return;
        patchState(store, { messages: [...store.messages(), message] });
      });

      hub.decisionReceived$.subscribe((decision) => {
        // Tenant-wide pending list, independent of which conversation is open.
        const pending = store.pendingDecisions();
        const index = pending.findIndex((d) => d.id === decision.id);
        patchState(store, {
          pendingDecisions:
            decision.status === "suggested"
              ? index >= 0
                ? pending.map((d) => (d.id === decision.id ? decision : d))
                : [...pending, decision]
              : pending.filter((d) => d.id !== decision.id),
        });

        // Also patch the open conversation's transcript if the decision belongs to one of its turns.
        if (!store.sessions().some((s) => s.id === decision.sessionId)) return;
        const existing = store.decisions();
        const existingIndex = existing.findIndex((d) => d.id === decision.id);
        patchState(store, {
          decisions:
            existingIndex >= 0
              ? existing.map((d) => (d.id === decision.id ? decision : d))
              : [...existing, decision],
        });
      });

      hub.turnUpdateReceived$.subscribe((update) => {
        if (update.conversationId !== store.currentConversation()?.id) return;

        if (update.status === "running") {
          if (store.turnStatus() !== "running") {
            beginTurn();
          }
          patchState(store, { turnProgress: update.decisionCount, turnError: null });
          return;
        }

        endTurn(update.status === "failed" ? "failed" : "idle", update.errorMessage);
        void loadQuota();
        void reconcileConversation();
      });

      const loadHistoryPage = async (page: number): Promise<void> => {
        const result = await dispatchApi.fetchHistoryPage(page, HistoryPageSize);
        if (!result) return;
        patchState(store, {
          conversations:
            page === 1 ? (result.value ?? []) : [...store.conversations(), ...(result.value ?? [])],
          historyPage: page,
          hasMoreHistory: page < (result.totalPages ?? page),
        });
      };

      const loadConversations = (): Promise<void> => loadHistoryPage(1);

      return {
        /** Page bootstrap: connects the hub, loads the sidebar + right panel + most recent chat. */
        async init(): Promise<void> {
          void hub.acquireDispatchBoard(destroyRef);
          void loadQuota();
          void loadTrucks();
          void refreshPendingDecisions();

          patchState(store, { loading: true });
          await loadConversations();
          const mostRecent = store.conversations()[0];
          if (mostRecent?.id) {
            await this.openConversation(mostRecent.id);
          }
          patchState(store, { loading: false });
        },

        loadConversations,

        async openConversation(conversationId: string): Promise<void> {
          patchState(store, { loading: true });
          const conversation = await dispatchApi.fetchConversation(conversationId);
          if (conversation) {
            patchState(store, { turnError: null, turnProgress: null });
            applyConversation(conversation);
          }
          patchState(store, { loading: false });
        },

        /** Re-syncs the open conversation in place, without the load spinner. */
        reconcile: reconcileConversation,

        loadMoreConversations(): Promise<void> {
          return loadHistoryPage(store.historyPage() + 1);
        },

        startNewChat(): void {
          // The conversation is created lazily on first send - no empty rows.
          endTurn("idle");
          patchState(store, {
            currentConversation: null,
            messages: [],
            decisions: [],
            sessions: [],
          });
        },

        async sendMessage(text: string): Promise<void> {
          const trimmed = text.trim();
          if (!trimmed || store.sending() || store.isRunning()) return;

          patchState(store, { sending: true });
          try {
            const conversation = await currentOrNewConversation();
            if (!conversation) return;

            const isNewConversation = !store.conversations().some((c) => c.id === conversation.id);
            if (isNewConversation) {
              patchState(store, { conversations: [conversation, ...store.conversations()] });
            }

            // Deliberately untimestamped: it sorts last until the server's clock arrives, which is
            // where a just-sent message belongs anyway.
            const optimistic: AgentMessageDto = {
              id: `optimistic-${conversation.id}-${store.messages().length}`,
              conversationId: conversation.id,
              role: "user",
              text: trimmed,
            };
            patchState(store, { messages: [...store.messages(), optimistic] });
            beginTurn();

            const result = await dispatchApi.sendMessage(conversation.id!, trimmed);
            if (!result) {
              patchState(store, {
                messages: store.messages().filter((m) => m.id !== optimistic.id),
              });
              endTurn("idle");
              return;
            }

            patchState(store, {
              messages: store
                .messages()
                .map((m) =>
                  m.id === optimistic.id
                    ? { ...m, id: result.userMessageId, createdAt: result.userMessageCreatedAt }
                    : m,
                ),
            });
          } finally {
            patchState(store, { sending: false });
          }
        },

        /** Resends the last user message after a failed turn. */
        async retryTurn(): Promise<void> {
          if (store.sending() || store.isRunning()) return;
          const lastUserText = store
            .messages()
            .filter((m) => m.role === "user" && m.text)
            .at(-1)?.text;
          if (!lastUserText) return;
          endTurn("idle");
          await this.sendMessage(lastUserText);
        },

        /** Manual retry for a dead hub connection; also catches up on missed events. */
        async reconnect(): Promise<void> {
          await hub.acquireDispatchBoard(destroyRef);
          if (hub.isConnected) {
            await reconcileConversation();
            await refreshPendingDecisions();
          }
        },

        async cancelTurn(): Promise<void> {
          const conversationId = store.currentConversation()?.id;
          if (!conversationId) return;
          if (await dispatchApi.cancelTurn(conversationId)) {
            endTurn("idle");
          }
        },

        async renameConversation(conversationId: string, title: string): Promise<void> {
          const trimmed = title.trim();
          if (!trimmed || !(await dispatchApi.renameConversation(conversationId, trimmed))) return;

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
          if (!(await dispatchApi.deleteConversation(conversationId))) return;

          patchState(store, {
            conversations: store.conversations().filter((c) => c.id !== conversationId),
          });
          if (store.currentConversation()?.id === conversationId) {
            this.startNewChat();
          }
        },

        /** Refreshes the tenant-wide pending decisions (and the right panel + nav badge with them). */
        refreshPendingDecisions,

        toggleSidebar(): void {
          patchState(store, { sidebarCollapsed: !store.sidebarCollapsed() });
        },

        setSidebarCollapsed(collapsed: boolean): void {
          patchState(store, { sidebarCollapsed: collapsed });
        },

        toggleRightPanel(): void {
          const collapsed = !store.rightPanelCollapsed();
          patchState(store, { rightPanelCollapsed: collapsed });
          persistRightPanelCollapsed(collapsed);
        },
      };
    },
  ),
);
