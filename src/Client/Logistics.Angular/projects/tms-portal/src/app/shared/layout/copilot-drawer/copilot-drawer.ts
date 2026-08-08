import { CdkTrapFocus } from "@angular/cdk/a11y";
import {
  Component,
  computed,
  effect,
  inject,
  signal,
  viewChild,
  type ElementRef,
} from "@angular/core";
import type { AgentDecisionDto, AgentMessageDto } from "@logistics/shared/api";
import { LayoutService } from "@logistics/shared/services";
import { Icon, UiButton, UiTooltip } from "@logistics/shared/ui";
import { CopilotStore, type QuotaNotice } from "@/core/store";
import {
  ChatComposer,
  ChatMessage,
  DecisionActionsService,
  RejectDecisionDialog,
} from "@/shared/components";
import { CopilotActionCard } from "./copilot-action-card/copilot-action-card";
import { COPILOT_COMMANDS } from "./copilot-commands";
import { CopilotHistory } from "./copilot-history/copilot-history";

type StreamItem =
  | { kind: "message"; at: string; message: AgentMessageDto }
  | { kind: "decision"; at: string; decision: AgentDecisionDto };

/** Scroll distance from the bottom under which auto-scroll stays engaged. */
const ScrollPinThresholdPx = 48;
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
    CdkTrapFocus,
    ChatComposer,
    ChatMessage,
    CopilotActionCard,
    CopilotHistory,
    Icon,
    RejectDecisionDialog,
    UiButton,
    UiTooltip,
  ],
})
export class CopilotDrawer {
  protected readonly store = inject(CopilotStore);
  protected readonly actions = inject(DecisionActionsService);
  protected readonly layoutService = inject(LayoutService);

  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");
  private readonly composer = viewChild(ChatComposer);

  protected readonly copilotCommands = COPILOT_COMMANDS;

  private previouslyFocused: HTMLElement | null = null;

  /** False while the user has scrolled up to read back - new messages must not yank them down. */
  protected readonly pinnedToBottom = signal(true);

  protected readonly noticeClasses: Record<QuotaNotice["severity"], string> = {
    blocked: "border-danger/30 bg-danger/10 text-danger",
    overage: "border-warning/30 bg-warning/15 text-warning",
    info: "border-border bg-warning/10 text-muted-foreground",
  };

  protected readonly suggestedPrompts = [
    "Which loads were delivered last week?",
    "Show unpaid invoices",
    "Any trucks free tomorrow?",
    "What did we spend on fuel this month?",
  ];

  /** Messages and action cards interleaved chronologically; "9999" catches missing createdAt. */
  protected readonly stream = computed<StreamItem[]>(() => {
    const items: StreamItem[] = [
      ...this.store
        .messages()
        .map((message) => ({ kind: "message" as const, at: message.createdAt ?? "9999", message })),
      ...this.store.decisions().map((decision) => ({
        kind: "decision" as const,
        at: decision.createdAt ?? "9999",
        decision,
      })),
    ];
    return items.sort((a, b) => (a.at < b.at ? -1 : a.at > b.at ? 1 : 0));
  });

  /** Feeds the aria-live region. */
  protected readonly liveAnnouncement = computed(() => {
    const status = this.store.turnStatus();
    if (status === "running") return "Copilot is responding";
    if (status === "failed") return "Copilot turn failed";
    const last = this.store.messages().at(-1);
    return last?.role === "assistant" ? "Copilot replied" : "";
  });

  constructor() {
    this.actions.configure("copilot");

    effect(() => {
      this.stream();
      this.store.turnStatus();
      if (!this.pinnedToBottom()) return;
      const container = this.messagesContainer()?.nativeElement;
      if (container) {
        queueMicrotask(() => container.scrollTo({ top: container.scrollHeight }));
      }
    });

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

  protected onMessagesScroll(): void {
    const el = this.messagesContainer()?.nativeElement;
    if (!el) return;
    this.pinnedToBottom.set(
      el.scrollHeight - el.scrollTop - el.clientHeight < ScrollPinThresholdPx,
    );
  }

  protected scrollToBottom(): void {
    const el = this.messagesContainer()?.nativeElement;
    if (!el) return;
    el.scrollTo({ top: el.scrollHeight });
    this.pinnedToBottom.set(true);
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
