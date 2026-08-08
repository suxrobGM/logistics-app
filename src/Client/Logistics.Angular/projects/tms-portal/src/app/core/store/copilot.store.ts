import { computed, inject } from "@angular/core";
import { Permission } from "@logistics/shared";
import { ErrorCodes } from "@logistics/shared/errors";
import { FeatureService, PermissionService, ToastService } from "@logistics/shared/services";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import { CopilotApiService } from "@/core/services/copilot-api.service";
import { CopilotHubService } from "@/core/services/copilot-hub.service";
import { UpgradePromptService } from "@/core/services/upgrade-prompt.service";
import { withAgentChat } from "./agent-chat.feature";
import {
  clampDrawerWidth,
  DefaultDrawerWidth,
  persistDrawerWidth,
  readStoredDrawerWidth,
} from "./copilot-store.utils";

type CopilotView = "chat" | "history";

/** Drawer chrome; the conversation itself lives in {@link withAgentChat}. */
interface CopilotDrawerState {
  open: boolean;
  view: CopilotView;
  unreadCount: number;
  drawerWidth: number;
}

const initialDrawerState: CopilotDrawerState = {
  open: false,
  view: "chat",
  unreadCount: 0,
  drawerWidth: readStoredDrawerWidth(),
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
  withState(initialDrawerState),
  withAgentChat<CopilotDrawerState>({ api: CopilotApiService, historyPageSize: 20 }),

  withComputed(
    (
      store,
      featureService = inject(FeatureService),
      permissionService = inject(PermissionService),
      copilotHub = inject(CopilotHubService),
    ) => ({
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
    }),
  ),

  withMethods(
    (
      store,
      copilotHub = inject(CopilotHubService),
      featureService = inject(FeatureService),
      permissionService = inject(PermissionService),
      upgradePrompt = inject(UpgradePromptService),
      toast = inject(ToastService),
    ) => {
      // Subscribed once at store creation (root store, never destroyed); the hub connects lazily
      // on first drawer open.
      copilotHub.messageReceived$.subscribe((message) => {
        if (message.conversationId !== store.currentConversation()?.id) return;

        store.appendMessage(message);
        if (!store.open()) {
          patchState(store, { unreadCount: store.unreadCount() + 1 });
        }
      });

      copilotHub.decisionReceived$.subscribe((decision) => store.upsertDecision(decision));

      copilotHub.turnUpdateReceived$.subscribe((update) => {
        if (update.conversationId !== store.currentConversation()?.id) return;
        store.applyTurnUpdate(update);
      });

      const openConversation = async (conversationId: string): Promise<void> => {
        patchState(store, { view: "chat" });
        if (!(await store.loadConversation(conversationId))) {
          patchState(store, { view: "history" });
        }
      };

      const openDrawer = async (): Promise<void> => {
        if (!permissionService.hasPermission(Permission.Copilot.View)) return;

        patchState(store, { open: true, unreadCount: 0 });
        void store.loadQuota();

        // The transcript needs nothing from the hub, so the handshake RTT must not gate it.
        if (store.currentConversation()) {
          await copilotHub.acquire();
          return;
        }

        patchState(store, { loading: true });
        await Promise.all([copilotHub.acquire(), store.loadConversations()]);
        const mostRecent = store.conversations()[0];
        if (mostRecent?.id) {
          await openConversation(mostRecent.id);
        }
        patchState(store, { loading: false });
      };

      return {
        openConversation,
        openDrawer,

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
          void store.loadConversations();
        },

        startNewChat(): void {
          patchState(store, { view: "chat" });
          store.resetConversation();
        },

        /** Manual retry for a dead hub connection; also catches up on missed events. */
        async reconnect(): Promise<void> {
          await copilotHub.acquire();
          if (copilotHub.isConnected) {
            await store.reconcile();
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
