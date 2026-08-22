import { CdkTrapFocus } from "@angular/cdk/a11y";
import { Component, computed, effect, inject, viewChild, type ElementRef } from "@angular/core";
import { Permission } from "@logistics/shared";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { LayoutService } from "@logistics/shared/services";
import { Icon, Spinner, Stack, UiButton, UiTooltip } from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { CopilotStore } from "@/core/store";
import {
  AgentDecisionCard,
  ChatComposer,
  ChatMessage,
  ConversationList,
  DecisionActionsService,
  pinnedScroll,
  QuotaNotice,
  RealtimeBanner,
  RejectDecisionDialog,
  ScrollToBottom,
  TurnError,
  TurnStatus,
} from "@/shared/components";
import { COPILOT_COMMANDS } from "./copilot-commands";
import { buildCopilotStream } from "./copilot-stream.utils";
import { CopilotWelcome } from "./copilot-welcome/copilot-welcome";

const ResizeKeyStepPx = 16;

/**
 * The copilot chat drawer. Non-modal on purpose (`ui-drawer` is modal) so the page behind stays
 * interactive mid-conversation: a docked flex child on desktop, a full-screen takeover above the
 * z-40 mobile header on mobile.
 *
 * Always mounted in the shell for the Ctrl/Cmd+I shortcut, but the body renders only while open -
 * so all state lives in the root CopilotStore.
 */
@Component({
  selector: "app-copilot-drawer",
  templateUrl: "./copilot-drawer.html",
  providers: [DecisionActionsService],
  host: {
    // contents: the @if-mounted aside must be the shell row's direct flex item.
    class: "contents",
    "(document:keydown)": "onGlobalKeydown($event)",
  },
  imports: [
    AgentDecisionCard,
    CdkTrapFocus,
    ChatComposer,
    ChatMessage,
    ConversationList,
    CopilotWelcome,
    Icon,
    QuotaNotice,
    RealtimeBanner,
    RejectDecisionDialog,
    ScrollToBottom,
    Spinner,
    Stack,
    TurnError,
    TurnStatus,
    UiButton,
    UiTooltip,
  ],
})
export class CopilotDrawer {
  protected readonly store = inject(CopilotStore);
  protected readonly actions = inject(DecisionActionsService);
  protected readonly layoutService = inject(LayoutService);

  protected readonly currentUserId = inject(AuthService).userId;

  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");
  private readonly composer = viewChild(ChatComposer);

  protected readonly copilotCommands = COPILOT_COMMANDS;
  protected readonly copilotManage = Permission.Copilot.Manage;

  private previouslyFocused: HTMLElement | null = null;

  protected readonly stream = computed(() =>
    buildCopilotStream(this.store.messages(), this.store.decisions()),
  );

  /** Feeds the aria-live region. */
  protected readonly liveAnnouncement = computed(() => {
    const status = this.store.turnStatus();
    if (status === "running") return "Copilot is responding";
    if (status === "failed") return "Copilot turn failed";
    const last = this.store.messages().at(-1);
    return last?.role === "assistant" ? "Copilot replied" : "";
  });

  protected readonly scroll = pinnedScroll(this.messagesContainer, () => {
    this.stream();
    this.store.turnStatus();
  });

  constructor() {
    this.actions.configure("copilot");

    effect(() => {
      if (this.store.open()) {
        this.previouslyFocused = document.activeElement as HTMLElement | null;
        queueMicrotask(() => this.composer()?.focus());
      } else {
        this.previouslyFocused?.focus();
        this.previouslyFocused = null;
      }
    });

    // iOS still overscrolls the body behind the fixed mobile takeover without this.
    effect(() => {
      const lock = this.store.open() && this.layoutService.isMobile();
      document.body.style.overflow = lock ? "hidden" : "";
    });
  }

  protected onGlobalKeydown(event: KeyboardEvent): void {
    if (
      (event.ctrlKey || event.metaKey) &&
      !event.shiftKey &&
      !event.altKey &&
      event.key.toLowerCase() === "i"
    ) {
      // Quill owns Ctrl+I (italic) inside editable targets.
      if (this.isEditableTarget(event.target)) return;
      event.preventDefault();
      void this.store.toggle();
      return;
    }

    // The reject dialog owns Escape while it is open.
    if (event.key === "Escape" && this.store.open() && !this.actions.showRejectDialog()) {
      this.store.closeDrawer();
    }
  }

  protected startResize(event: PointerEvent): void {
    event.preventDefault();
    const startX = event.clientX;
    const startWidth = this.store.drawerWidth();

    const onMove = (e: PointerEvent): void =>
      this.store.resizeDrawerTo(startWidth + (startX - e.clientX));

    const onUp = (): void => {
      document.removeEventListener("pointermove", onMove);
      document.removeEventListener("pointerup", onUp);
      document.body.style.userSelect = "";
      this.store.commitDrawerWidth();
    };

    document.addEventListener("pointermove", onMove);
    document.addEventListener("pointerup", onUp);
    document.body.style.userSelect = "none";
  }

  protected onResizeKeydown(event: KeyboardEvent): void {
    if (event.key === "ArrowLeft") {
      event.preventDefault();
      this.store.setDrawerWidth(this.store.drawerWidth() + ResizeKeyStepPx);
    } else if (event.key === "ArrowRight") {
      event.preventDefault();
      this.store.setDrawerWidth(this.store.drawerWidth() - ResizeKeyStepPx);
    }
  }

  protected onComposerCommand(commandName: string): void {
    const command = COPILOT_COMMANDS.find((c) => c.name === commandName);
    if (!command) return;

    if (command.action === "startNewChat") {
      this.store.startNewChat();
    } else {
      this.store.showHistory();
    }
  }

  protected approve(decision: AgentDecisionDto): void {
    this.actions.approve(decision, () => this.store.reconcile());
  }

  protected reject(decision: AgentDecisionDto): void {
    this.actions.reject(decision, () => this.store.reconcile());
  }

  private isEditableTarget(target: EventTarget | null): boolean {
    const el = target as HTMLElement | null;
    return !!el && (el.tagName === "INPUT" || el.tagName === "TEXTAREA" || el.isContentEditable);
  }
}
