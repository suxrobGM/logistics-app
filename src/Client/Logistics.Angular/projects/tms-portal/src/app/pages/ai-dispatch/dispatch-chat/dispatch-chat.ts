import { Component, inject } from "@angular/core";
import { RouterLink } from "@angular/router";
import { LayoutService } from "@logistics/shared/services";
import { Icon, Stack, Typography, UiButton, UiDrawer, UiTooltip } from "@logistics/shared/ui";
import { QuotaNoticeClasses } from "@/core/store";
import {
  ChatComposer,
  ConversationList,
  DecisionActionsService,
  RealtimeBanner,
  RejectDecisionDialog,
} from "@/shared/components";
import { DispatchRightPanel } from "../components/dispatch-right-panel/dispatch-right-panel";
import { DispatchTranscript } from "../components/dispatch-transcript/dispatch-transcript";
import { PLAN_ASSIGNMENTS_PROMPT } from "../components/dispatch-welcome/quick-actions";
import { DispatchChatStore } from "../store/dispatch-chat.store";

/**
 * Full-page AI dispatch chat: the transcript (messages + per-turn tool-activity timelines) with a
 * conversation drawer on the left and a fleet/decisions panel on the right.
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
    RealtimeBanner,
    RejectDecisionDialog,
    RouterLink,
    Stack,
    Typography,
    UiButton,
    UiDrawer,
    UiTooltip,
  ],
})
export class DispatchChat {
  protected readonly store = inject(DispatchChatStore);
  protected readonly layoutService = inject(LayoutService);

  protected readonly noticeClasses = QuotaNoticeClasses;
  protected readonly planAssignmentsPrompt = PLAN_ASSIGNMENTS_PROMPT;

  constructor() {
    void this.store.init();
  }

  protected selectConversation(conversationId: string): void {
    void this.store.openConversation(conversationId);
    this.store.setSidebarCollapsed(true);
  }

  protected newChat(): void {
    this.store.startNewChat();
    this.store.setSidebarCollapsed(true);
  }
}
