import { Component, computed, effect, inject, viewChild, type ElementRef } from "@angular/core";
import type { AICopilotMessageDto, AIDispatchDecisionDto } from "@logistics/shared/api";
import { Icon, UiButton, UiTooltip } from "@logistics/shared/ui";
import { CopilotStore } from "@/core/store";
import { DecisionActionsService, RejectDecisionDialog } from "@/shared/components";
import { CopilotActionCard } from "./copilot-action-card/copilot-action-card";
import { CopilotComposer } from "./copilot-composer/copilot-composer";
import { CopilotHistory } from "./copilot-history/copilot-history";
import { CopilotMessage } from "./copilot-message/copilot-message";

type StreamItem =
  | { kind: "message"; at: string; message: AICopilotMessageDto }
  | { kind: "decision"; at: string; decision: AIDispatchDecisionDto };

/**
 * The copilot chat drawer. Non-modal on purpose (`ui-drawer` is modal) so the page behind stays
 * interactive mid-conversation: a docked flex child on desktop, a full-screen takeover above the
 * z-40 mobile header on mobile.
 *
 * Always mounted in the shell for the Ctrl+I shortcut, but the body renders only while open - so
 * all state lives in the root CopilotStore.
 */
@Component({
  selector: "app-copilot-drawer",
  templateUrl: "./copilot-drawer.html",
  providers: [DecisionActionsService],
  host: {
    "(document:keydown)": "onGlobalKeydown($event)",
  },
  imports: [
    CopilotActionCard,
    CopilotComposer,
    CopilotHistory,
    CopilotMessage,
    Icon,
    RejectDecisionDialog,
    UiButton,
    UiTooltip,
  ],
})
export class CopilotDrawer {
  protected readonly store = inject(CopilotStore);
  protected readonly actions = inject(DecisionActionsService);

  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");

  /** Messages and action cards interleaved chronologically, optimistic entries last. */
  protected readonly stream = computed<StreamItem[]>(() => {
    const items: StreamItem[] = [
      ...this.store
        .messages()
        .map((message) => ({ kind: "message" as const, at: message.createdAt ?? "9999", message })),
      ...this.store
        .decisions()
        .map((decision) => ({ kind: "decision" as const, at: decision.createdAt ?? "", decision })),
    ];
    return items.sort((a, b) => a.at.localeCompare(b.at));
  });

  constructor() {
    this.actions.configure("copilot");

    effect(() => {
      this.stream();
      this.store.turnStatus();
      const container = this.messagesContainer()?.nativeElement;
      if (container) {
        queueMicrotask(() => container.scrollTo({ top: container.scrollHeight }));
      }
    });
  }

  protected onGlobalKeydown(event: KeyboardEvent): void {
    if (event.ctrlKey && !event.shiftKey && !event.altKey && event.key.toLowerCase() === "i") {
      event.preventDefault();
      void this.store.toggle();
      return;
    }

    // The reject dialog owns Escape while it is open.
    if (event.key === "Escape" && this.store.open() && !this.actions.showRejectDialog()) {
      this.store.closeDrawer();
    }
  }

  protected approve(decision: AIDispatchDecisionDto): void {
    this.actions.approve(decision, () => this.refreshConversation());
  }

  protected reject(decision: AIDispatchDecisionDto): void {
    this.actions.reject(decision, () => this.refreshConversation());
  }

  private async refreshConversation(): Promise<void> {
    const conversationId = this.store.currentConversation()?.id;
    if (conversationId) {
      await this.store.openConversation(conversationId);
    }
  }
}
