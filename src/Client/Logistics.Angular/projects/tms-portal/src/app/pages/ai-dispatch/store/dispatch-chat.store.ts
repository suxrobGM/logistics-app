import { computed, DestroyRef, effect, inject } from "@angular/core";
import type { AgentDecisionDto, AgentSessionDto, TruckDto } from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { patchState, signalStore, withComputed, withMethods, withState } from "@ngrx/signals";
import type { AgentTurnUpdate } from "@/core/services/agent-chat.contracts";
import { AIDispatchHubService } from "@/core/services/ai-dispatch-hub.service";
import { DispatchApiService } from "@/core/services/dispatch-api.service";
import { DispatchBadgeService } from "@/core/services/dispatch-badge.service";
import { withAgentChat } from "@/core/store/agent-chat.feature";
import {
  persistRightPanelCollapsed,
  readStoredRightPanelCollapsed,
} from "./dispatch-chat-store.utils";

/** Board chrome and fleet context; the conversation itself lives in {@link withAgentChat}. */
interface DispatchBoardState {
  /** One per turn, carrying the tool-activity timeline the transcript renders. */
  sessions: AgentSessionDto[];
  /** Tenant-wide write decisions awaiting approval - the right panel + sidebar nav badge. */
  pendingDecisions: AgentDecisionDto[];
  trucks: TruckDto[];
  sidebarCollapsed: boolean;
  rightPanelCollapsed: boolean;
}

const initialBoardState: DispatchBoardState = {
  sessions: [],
  pendingDecisions: [],
  trucks: [],
  sidebarCollapsed: true,
  rightPanelCollapsed: readStoredRightPanelCollapsed(),
};

/**
 * Page-scoped (provided by `DispatchChat`, not root) so the dispatch-board hub claim releases on
 * navigation - nothing outside the page needs this state. HTTP lives in `DispatchApiService`.
 */
export const DispatchChatStore = signalStore(
  withState(initialBoardState),
  withAgentChat<DispatchBoardState>({
    api: DispatchApiService,
    historyPageSize: 30,
    conversationExtras: (conversation) => ({ sessions: conversation?.sessions ?? [] }),
  }),

  withComputed((store, hub = inject(AIDispatchHubService)) => ({
    /** "connecting" is not "down" - the banner should only show for lost/failed connections. */
    realtimeDown: computed(() => {
      const state = hub.connectionState();
      return state === "disconnected" || state === "reconnecting";
    }),
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
  })),

  withMethods(
    (
      store,
      dispatchApi = inject(DispatchApiService),
      hub = inject(AIDispatchHubService),
      dispatchBadge = inject(DispatchBadgeService),
      destroyRef = inject(DestroyRef),
    ) => {
      effect(() => dispatchBadge.pendingCount.set(store.writeDecisions().length));

      const refreshPendingDecisions = async (): Promise<void> => {
        const pending = await dispatchApi.fetchPendingDecisions({ silent: true });
        if (pending) patchState(store, { pendingDecisions: pending });
      };

      /** The fleet map is the only consumer, so a collapsed right panel must not pay for 100 trucks. */
      const loadTrucksIfPanelOpen = async (): Promise<void> => {
        if (store.rightPanelCollapsed() || store.trucks().length > 0) return;
        patchState(store, { trucks: await dispatchApi.fetchAvailableTrucks() });
      };

      const setRightPanelCollapsed = (collapsed: boolean): void => {
        patchState(store, { rightPanelCollapsed: collapsed });
        persistRightPanelCollapsed(collapsed);
        void loadTrucksIfPanelOpen();
      };

      /**
       * The turn's session must land in `sessions` from the update itself, not from a later
       * refetch: a decision whose session we have never seen is not part of the open conversation,
       * so a running turn's tool activity would stay invisible until the turn ended.
       */
      const upsertSession = (update: AgentTurnUpdate): AgentSessionDto[] => {
        const patch: AgentSessionDto = {
          id: update.sessionId,
          status: update.status,
          totalTokensUsed: update.totalTokensUsed,
          decisionCount: update.decisionCount,
          errorMessage: update.errorMessage,
        };
        const existing = store.sessions();
        return existing.some((s) => s.id === update.sessionId)
          ? existing.map((s) => (s.id === update.sessionId ? { ...s, ...patch } : s))
          : [...existing, patch];
      };

      // No takeUntilDestroyed: the store is page-scoped, so these die with the page.
      hub.messageReceived$.subscribe((message) => {
        if (message.conversationId !== store.currentConversation()?.id) return;
        store.appendMessage(message);
      });

      hub.decisionReceived$.subscribe((decision) => {
        // Tenant-wide pending list, independent of which conversation is open.
        const pending = store.pendingDecisions();
        patchState(store, {
          pendingDecisions:
            decision.status === "suggested"
              ? pending.some((d) => d.id === decision.id)
                ? pending.map((d) => (d.id === decision.id ? decision : d))
                : [...pending, decision]
              : pending.filter((d) => d.id !== decision.id),
        });

        if (store.sessions().some((s) => s.id === decision.sessionId)) {
          store.upsertDecision(decision);
        }
      });

      hub.turnUpdateReceived$.subscribe((update) => {
        if (update.conversationId !== store.currentConversation()?.id) return;
        patchState(store, { sessions: upsertSession(update) });
        store.applyTurnUpdate(update);
      });

      return {
        /** Page bootstrap: connects the hub, loads the sidebar + right panel + most recent chat. */
        async init(): Promise<void> {
          void hub.acquireDispatchBoard(destroyRef);
          void store.loadQuota();
          void loadTrucksIfPanelOpen();
          void refreshPendingDecisions();

          patchState(store, { loading: true });
          await store.loadConversations();
          const mostRecent = store.conversations()[0];
          if (mostRecent?.id) {
            await store.loadConversation(mostRecent.id);
          }
          patchState(store, { loading: false });
        },

        openConversation(conversationId: string): Promise<boolean> {
          return store.loadConversation(conversationId);
        },

        startNewChat(): void {
          store.resetConversation();
        },

        /** Manual retry for a dead hub connection; also catches up on missed events. */
        async reconnect(): Promise<void> {
          await hub.acquireDispatchBoard(destroyRef);
          if (hub.isConnected) {
            await store.reconcile();
            await refreshPendingDecisions();
          }
        },

        /** Refreshes the tenant-wide pending decisions (and the right panel + nav badge with them). */
        refreshPendingDecisions,

        /** A decision resolved from either the transcript timeline or the right panel - refresh both. */
        async onDecisionResolved(): Promise<void> {
          await Promise.all([store.reconcile(), refreshPendingDecisions()]);
        },

        toggleSidebar(): void {
          patchState(store, { sidebarCollapsed: !store.sidebarCollapsed() });
        },

        setSidebarCollapsed(collapsed: boolean): void {
          patchState(store, { sidebarCollapsed: collapsed });
        },

        setRightPanelCollapsed,

        toggleRightPanel(): void {
          setRightPanelCollapsed(!store.rightPanelCollapsed());
        },
      };
    },
  ),
);
