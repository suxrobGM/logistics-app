import { computed, inject } from "@angular/core";
import {
  Api,
  cancelCopilotTurn,
  createCopilotConversation,
  deleteCopilotConversation,
  getCopilotConversationById,
  getCopilotConversations,
  sendCopilotMessage,
  type AgentDecisionDto,
  type AICopilotConversationDto,
  type AICopilotMessageDto,
} from "@logistics/shared/api";
import { ErrorCodes, getApiError, isUpgradeError } from "@logistics/shared/errors";
import { FeatureService, ToastService } from "@logistics/shared/services";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { CopilotHubService } from "@/core/services/copilot-hub.service";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";

type CopilotView = "chat" | "history";
type TurnStatus = "idle" | "running" | "failed";

interface CopilotState {
  open: boolean;
  view: CopilotView;
  conversations: AICopilotConversationDto[];
  currentConversation: AICopilotConversationDto | null;
  messages: AICopilotMessageDto[];
  decisions: AgentDecisionDto[];
  turnStatus: TurnStatus;
  turnError: string | null;
  loading: boolean;
  sending: boolean;
  unreadCount: number;
  initialized: boolean;
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
  loading: false,
  sending: false,
  unreadCount: 0,
  initialized: false,
};

/**
 * Root-provided: the drawer body unmounts when closed and the launchers need the open/unread
 * state, so none of this can live in a component. Also the sole subscriber of CopilotHubService -
 * its setters are single-subscriber, a second registrant would steal them.
 */
export const CopilotStore = signalStore(
  { providedIn: "root" },
  withState(initialState),

  withComputed((store, featureService = inject(FeatureService)) => ({
    isRunning: computed(() => store.turnStatus() === "running"),
    hasUnread: computed(() => store.unreadCount() > 0),
    launcherVisible: computed(() => !featureService.isLocked("ai_copilot")),
  })),

  withMethods(
    (
      store,
      api = inject(Api),
      copilotHub = inject(CopilotHubService),
      featureService = inject(FeatureService),
      upgradePrompt = inject(UpgradePromptService),
      toast = inject(ToastService),
    ) => {
      const setupRealtimeHandlers = (): void => {
        if (store.initialized()) return;
        patchState(store, { initialized: true });

        copilotHub.onReceiveCopilotMessage = (message) => {
          if (message.conversationId !== store.currentConversation()?.id) return;

          patchState(store, { messages: [...store.messages(), message] });
          if (!store.open()) {
            patchState(store, { unreadCount: store.unreadCount() + 1 });
          }
        };

        copilotHub.onReceiveCopilotDecision = (decision) => {
          const existing = store.decisions();
          const index = existing.findIndex((d) => d.id === decision.id);
          patchState(store, {
            decisions:
              index >= 0
                ? existing.map((d) => (d.id === decision.id ? decision : d))
                : [...existing, decision],
          });
        };

        copilotHub.onReceiveCopilotTurnUpdate = (update) => {
          if (update.conversationId !== store.currentConversation()?.id) return;

          if (update.status === "running") {
            patchState(store, { turnStatus: "running", turnError: null });
            return;
          }

          patchState(store, {
            turnStatus: update.status === "failed" ? "failed" : "idle",
            turnError: update.errorMessage,
          });
        };
      };

      const loadConversations = async (): Promise<void> => {
        const result = await api.invoke(getCopilotConversations, { Page: 1, PageSize: 20 });
        patchState(store, { conversations: result.value ?? [] });
      };

      const openConversation = async (conversationId: string): Promise<void> => {
        patchState(store, { loading: true, view: "chat" });
        try {
          const conversation = await api.invoke(getCopilotConversationById, { conversationId });
          patchState(store, {
            currentConversation: conversation,
            messages: conversation.messages ?? [],
            decisions: conversation.decisions ?? [],
            turnStatus: conversation.status === "running" ? "running" : "idle",
            turnError: null,
          });
        } finally {
          patchState(store, { loading: false });
        }
      };

      const openDrawer = async (): Promise<void> => {
        setupRealtimeHandlers();
        patchState(store, { open: true, unreadCount: 0 });
        await copilotHub.connect();

        if (!store.currentConversation()) {
          patchState(store, { loading: true });
          try {
            await loadConversations();
            const mostRecent = store.conversations()[0];
            if (mostRecent?.id) {
              await openConversation(mostRecent.id);
            }
          } finally {
            patchState(store, { loading: false });
          }
        }
      };

      const handleSendError = (error: unknown): void => {
        const apiError = getApiError(error);
        if (isUpgradeError(apiError?.errorCode)) {
          upgradePrompt.showUpgradePrompt(apiError!.errorCode!, apiError!.message);
        } else {
          toast.showError("Failed to send message");
        }
      };

      return {
        setupRealtimeHandlers,
        loadConversations,
        openConversation,
        openDrawer,

        closeDrawer(): void {
          patchState(store, { open: false });
        },

        async toggle(): Promise<void> {
          if (store.open()) {
            patchState(store, { open: false });
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

        startNewChat(): void {
          // The conversation is created lazily on first send - no empty rows.
          patchState(store, {
            view: "chat",
            currentConversation: null,
            messages: [],
            decisions: [],
            turnStatus: "idle",
            turnError: null,
          });
        },

        async sendMessage(text: string): Promise<void> {
          const trimmed = text.trim();
          if (!trimmed || store.sending() || store.isRunning()) return;

          patchState(store, { sending: true });
          try {
            let conversation = store.currentConversation();
            if (!conversation) {
              conversation = await api.invoke(createCopilotConversation);
              patchState(store, { currentConversation: conversation });
            }

            const optimistic: AICopilotMessageDto = {
              id: `optimistic-${conversation.id}-${store.messages().length}`,
              conversationId: conversation.id,
              role: "user",
              text: trimmed,
            };
            patchState(store, {
              messages: [...store.messages(), optimistic],
              turnStatus: "running",
              turnError: null,
            });

            try {
              const result = await api.invoke(sendCopilotMessage, {
                conversationId: conversation.id!,
                body: { conversationId: conversation.id!, text: trimmed },
              });
              patchState(store, {
                messages: store
                  .messages()
                  .map((m) => (m.id === optimistic.id ? { ...m, id: result.userMessageId } : m)),
              });
            } catch (error) {
              patchState(store, {
                messages: store.messages().filter((m) => m.id !== optimistic.id),
                turnStatus: "idle",
              });
              handleSendError(error);
            }
          } finally {
            patchState(store, { sending: false });
          }
        },

        async cancelTurn(): Promise<void> {
          const conversationId = store.currentConversation()?.id;
          if (!conversationId) return;
          await api.invoke(cancelCopilotTurn, { conversationId });
          patchState(store, { turnStatus: "idle" });
        },

        async deleteConversation(conversationId: string): Promise<void> {
          await api.invoke(deleteCopilotConversation, { conversationId });
          patchState(store, {
            conversations: store.conversations().filter((c) => c.id !== conversationId),
          });
          if (store.currentConversation()?.id === conversationId) {
            this.startNewChat();
          }
        },
      };
    },
  ),
);
