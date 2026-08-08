import { computed, inject } from "@angular/core";
import { Permission } from "@logistics/shared";
import type {
  AgentConversationDto,
  AgentDecisionDto,
  AgentMessageDto,
  AIQuotaStatusDto,
} from "@logistics/shared/api";
import { ErrorCodes } from "@logistics/shared/errors";
import {
  FeatureService,
  LocalizationService,
  PermissionService,
  ToastService,
} from "@logistics/shared/services";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { CopilotApiService } from "@/core/services/copilot-api.service";
import { CopilotHubService } from "@/core/services/copilot-hub.service";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";
import { buildQuotaNotice, TurnWatchdog } from "./agent-chat.helpers";
import {
  clampDrawerWidth,
  DefaultDrawerWidth,
  persistDrawerWidth,
  readStoredDrawerWidth,
} from "./copilot-store.utils";

type CopilotView = "chat" | "history";
type TurnStatus = "idle" | "running" | "failed";

const HistoryPageSize = 20;

interface CopilotState {
  open: boolean;
  view: CopilotView;
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
  unreadCount: number;
  historyPage: number;
  hasMoreHistory: boolean;
  drawerWidth: number;
  quota: AIQuotaStatusDto | null;
}

const initialState: CopilotState = {
  open: false,
  view: "chat",
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
  unreadCount: 0,
  historyPage: 1,
  hasMoreHistory: false,
  drawerWidth: readStoredDrawerWidth(),
  quota: null,
};

/**
 * Root-provided: the drawer body unmounts when closed and the launchers need the open/unread
 * state, so none of this can live in a component. The sole intended subscriber of
 * CopilotHubService - components read the store, never the hub. Being app-lifetime, it acquires the
 * hub without a DestroyRef and never releases.
 * HTTP lives in CopilotApiService; this store only orchestrates state.
 */
export const CopilotStore = signalStore(
  { providedIn: "root" },
  withState(initialState),

  withComputed(
    (
      store,
      featureService = inject(FeatureService),
      permissionService = inject(PermissionService),
      copilotHub = inject(CopilotHubService),
      localization = inject(LocalizationService),
    ) => ({
      isRunning: computed(() => store.turnStatus() === "running"),
      hasUnread: computed(() => store.unreadCount() > 0),
      launcherVisible: computed(
        () =>
          !featureService.isLocked("ai_copilot") &&
          permissionService.hasPermission(Permission.Copilot.View),
      ),
      /** "connecting" is not "down" - the banner should only show for lost/failed connections. */
      realtimeDown: computed(() => {
        const state = copilotHub.connectionState();
        return store.open() && (state === "disconnected" || state === "reconnecting");
      }),
      quotaNotice: computed(() =>
        buildQuotaNotice(store.quota(), (value) => localization.formatCurrency(value)),
      ),
      /** Owner opted for a hard pause and the budget is spent - the composer disables. */
      quotaBlocked: computed(() => store.quota()?.overageBlocked === true),
    }),
  ),

  withMethods(
    (
      store,
      copilotApi = inject(CopilotApiService),
      copilotHub = inject(CopilotHubService),
      featureService = inject(FeatureService),
      permissionService = inject(PermissionService),
      upgradePrompt = inject(UpgradePromptService),
      toast = inject(ToastService),
    ) => {
      const watchdog = new TurnWatchdog(
        () => void reconcileConversation(),
        () => patchState(store, { longRunning: true }),
      );

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
        patchState(store, {
          currentConversation: conversation,
          messages: conversation.messages ?? [],
          decisions: conversation.decisions ?? [],
        });
        // Guarded: re-running beginTurn mid-turn would reset the longRunning marker.
        if (conversation.status === "running") {
          if (store.turnStatus() !== "running") beginTurn();
        } else if (store.turnStatus() === "running") {
          endTurn("idle");
        }
      };

      /** Conversations are created lazily, on the first send rather than on drawer open. */
      const currentOrNewConversation = async (): Promise<AgentConversationDto | null> => {
        const existing = store.currentConversation();
        if (existing) return existing;

        const created = await copilotApi.createConversation();
        if (created) patchState(store, { currentConversation: created });
        return created;
      };

      /** Re-fetches the open conversation to recover state the hub failed to deliver. */
      const reconcileConversation = async (): Promise<void> => {
        const conversationId = store.currentConversation()?.id;
        if (!conversationId) return;
        const conversation = await copilotApi.fetchConversation(conversationId, { silent: true });
        if (!conversation || store.currentConversation()?.id !== conversationId) return;
        applyConversation(conversation);
      };

      const loadQuota = async (): Promise<void> => {
        const quota = await copilotApi.fetchQuota();
        if (quota) patchState(store, { quota });
      };

      // Subscribed once at store creation (root store, never destroyed); the hub connects lazily
      // on first drawer open.
      copilotHub.messageReceived$.subscribe((message) => {
        if (message.conversationId !== store.currentConversation()?.id) return;

        patchState(store, { messages: [...store.messages(), message] });
        if (!store.open()) {
          patchState(store, { unreadCount: store.unreadCount() + 1 });
        }
      });

      copilotHub.decisionReceived$.subscribe((decision) => {
        const existing = store.decisions();
        const index = existing.findIndex((d) => d.id === decision.id);
        patchState(store, {
          decisions:
            index >= 0
              ? existing.map((d) => (d.id === decision.id ? decision : d))
              : [...existing, decision],
        });
      });

      copilotHub.turnUpdateReceived$.subscribe((update) => {
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
      });

      const loadHistoryPage = async (page: number): Promise<void> => {
        const result = await copilotApi.fetchHistoryPage(page, HistoryPageSize);
        if (!result) return;
        patchState(store, {
          conversations:
            page === 1 ? (result.value ?? []) : [...store.conversations(), ...(result.value ?? [])],
          historyPage: page,
          hasMoreHistory: page < (result.totalPages ?? page),
        });
      };

      const loadConversations = (): Promise<void> => loadHistoryPage(1);

      const openConversation = async (conversationId: string): Promise<void> => {
        patchState(store, { loading: true, view: "chat" });
        const conversation = await copilotApi.fetchConversation(conversationId);
        if (conversation) {
          patchState(store, { turnError: null, turnProgress: null });
          applyConversation(conversation);
        } else {
          patchState(store, { view: "history" });
        }
        patchState(store, { loading: false });
      };

      const openDrawer = async (): Promise<void> => {
        if (!permissionService.hasPermission(Permission.Copilot.View)) return;

        patchState(store, { open: true, unreadCount: 0 });
        void loadQuota();

        // The transcript needs nothing from the hub, so the handshake RTT must not gate it.
        if (store.currentConversation()) {
          await copilotHub.acquire();
          return;
        }

        patchState(store, { loading: true });
        await Promise.all([copilotHub.acquire(), loadConversations()]);
        const mostRecent = store.conversations()[0];
        if (mostRecent?.id) {
          await openConversation(mostRecent.id);
        }
        patchState(store, { loading: false });
      };

      return {
        loadConversations,
        openConversation,
        openDrawer,

        /** Re-syncs the open conversation in place, without the load spinner or a view switch. */
        reconcile: reconcileConversation,

        closeDrawer(): void {
          // Turn timers keep running while closed so the finished turn still reconciles.
          patchState(store, { open: false });
        },

        async toggle(): Promise<void> {
          if (store.open()) {
            this.closeDrawer();
          } else {
            await openDrawer();
          }
        },

        /** Launcher entry point: opens the drawer, or upsells when the plan lacks the feature. */
        async openOrUpsell(): Promise<void> {
          if (featureService.isEnabled("ai_copilot")) {
            await this.toggle();
            return;
          }

          const status = featureService.getAllFeatures().find((f) => f.feature === "ai_copilot");

          // Upsell only when the plan actually lacks the feature; a tenant that has it in-plan
          // (or needs no plan) but toggled it off gets pointed at settings instead.
          if (!status || status.isIncludedInPlan) {
            toast.showInfo("The AI Copilot is disabled. Enable it in Settings → Features.");
          } else {
            upgradePrompt.showUpgradePrompt(
              ErrorCodes.FeatureNotInPlan,
              "The AI Copilot is not included in your current plan.",
            );
          }
        },

        showHistory(): void {
          patchState(store, { view: "history" });
          void loadConversations();
        },

        loadMoreConversations(): Promise<void> {
          return loadHistoryPage(store.historyPage() + 1);
        },

        startNewChat(): void {
          // The conversation is created lazily on first send - no empty rows.
          endTurn("idle");
          patchState(store, {
            view: "chat",
            currentConversation: null,
            messages: [],
            decisions: [],
          });
        },

        async sendMessage(text: string): Promise<void> {
          const trimmed = text.trim();
          if (!trimmed || store.sending() || store.isRunning()) return;

          patchState(store, { sending: true });
          try {
            const conversation = await currentOrNewConversation();
            if (!conversation) return;

            // Deliberately untimestamped: it sorts last until the server's clock arrives, which is
            // where a just-sent message belongs anyway. A browser clock here would lose to the
            // server's on the reply and park the message below its own answer.
            const optimistic: AgentMessageDto = {
              id: `optimistic-${conversation.id}-${store.messages().length}`,
              conversationId: conversation.id,
              role: "user",
              text: trimmed,
            };
            patchState(store, { messages: [...store.messages(), optimistic] });
            beginTurn();

            const result = await copilotApi.sendMessage(conversation.id!, trimmed);
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
          await copilotHub.acquire();
          if (copilotHub.isConnected) {
            await reconcileConversation();
          }
        },

        async cancelTurn(): Promise<void> {
          const conversationId = store.currentConversation()?.id;
          if (!conversationId) return;
          if (await copilotApi.cancelTurn(conversationId)) {
            endTurn("idle");
          }
        },

        async renameConversation(conversationId: string, title: string): Promise<void> {
          const trimmed = title.trim();
          if (!trimmed || !(await copilotApi.renameConversation(conversationId, trimmed))) return;

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
          if (!(await copilotApi.deleteConversation(conversationId))) return;

          patchState(store, {
            conversations: store.conversations().filter((c) => c.id !== conversationId),
          });
          if (store.currentConversation()?.id === conversationId) {
            this.startNewChat();
          }
        },

        /** Drag-time update. Deliberately does not persist - see {@link commitDrawerWidth}. */
        resizeDrawerTo(width: number): void {
          patchState(store, { drawerWidth: clampDrawerWidth(width) });
        },

        /**
         * Persists the width a drag settled on. Separate from the move handler because
         * `localStorage.setItem` is a synchronous disk-backed write and pointermove fires several
         * times per frame.
         */
        commitDrawerWidth(): void {
          persistDrawerWidth(store.drawerWidth());
        },

        setDrawerWidth(width: number): void {
          this.resizeDrawerTo(width);
          this.commitDrawerWidth();
        },

        resetDrawerWidth(): void {
          this.setDrawerWidth(DefaultDrawerWidth);
        },
      };
    },
  ),
);
