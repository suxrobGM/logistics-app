import { Component, effect, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { LayoutService } from "@logistics/shared/services";
import { Icon, Stack, Typography, UiButton, UiTooltip } from "@logistics/shared/ui";
import { QuotaNoticeClasses } from "@/core/store";
import {
  ChatComposer,
  ConversationList,
  DecisionActionsService,
  RejectDecisionDialog,
} from "@/shared/components";
import { DispatchRightPanel } from "../components/dispatch-right-panel/dispatch-right-panel";
import { DispatchTranscript } from "../components/dispatch-transcript/dispatch-transcript";
import { DispatchChatStore } from "../store/dispatch-chat.store";

/**
 * Full-page, Claude-Code-style AI dispatch chat: a persistent conversation sidebar, the transcript
 * (messages + per-turn tool-activity timelines), and a right panel (fleet map, pending write
 * decisions, AI quota). Replaces the old sessions-list/session-details dashboard pages.
 */
@Component({
  selector: "app-dispatch-chat",
  templateUrl: "./dispatch-chat.html",
  providers: [DispatchChatStore, DecisionActionsService],
  imports: [
    ChatComposer,
    ConversationList,
    DispatchRightPanel,
    DispatchTranscript,
    Icon,
    RejectDecisionDialog,
    RouterLink,
    Stack,
    Typography,
    UiButton,
    UiTooltip,
  ],
})
export class DispatchChat {
  protected readonly store = inject(DispatchChatStore);
  protected readonly actions = inject(DecisionActionsService);
  protected readonly layoutService = inject(LayoutService);

  protected readonly noticeClasses = QuotaNoticeClasses;

  constructor() {
    // Sidebar starts collapsed on mobile; a later manual toggle is left alone.
    effect(() => {
      if (this.layoutService.isMobile()) {
        this.store.setSidebarCollapsed(true);
      }
    });

    void this.store.init();
  }

  protected selectConversation(conversationId: string): void {
    void this.store.openConversation(conversationId);
    if (this.layoutService.isMobile()) {
      this.store.setSidebarCollapsed(true);
    }
  }

  protected newChat(): void {
    this.store.startNewChat();
    if (this.layoutService.isMobile()) {
      this.store.setSidebarCollapsed(true);
    }
  }

  protected approve(decision: AgentDecisionDto): void {
    this.actions.approve(decision, () => this.onDecisionResolved());
  }

  protected reject(decision: AgentDecisionDto): void {
    this.actions.reject(decision, () => this.onDecisionResolved());
  }

  /** A decision resolved from either the transcript timeline or the right panel - refresh both. */
  private async onDecisionResolved(): Promise<void> {
    await Promise.all([this.store.reconcile(), this.store.refreshPendingDecisions()]);
  }
}
