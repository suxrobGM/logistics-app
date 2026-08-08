import {
  Component,
  computed,
  effect,
  inject,
  signal,
  viewChild,
  type ElementRef,
} from "@angular/core";
import { Alert, EmptyState, Spinner, Stack, Typography, UiButton } from "@logistics/shared/ui";
import { ChatMessage } from "@/shared/components";
import { DispatchChatStore } from "../../store/dispatch-chat.store";
import { DispatchTurnTimeline } from "../dispatch-turn-timeline/dispatch-turn-timeline";
import { buildTranscriptStream, type TranscriptItem } from "./dispatch-transcript.utils";

/** Scroll distance from the bottom under which auto-scroll stays engaged. */
const ScrollPinThresholdPx = 48;

const SUGGESTED_PROMPTS = [
  "Plan assignments for all unassigned loads",
  "Find loads for my idle trucks",
  "Review today's HOS risks",
  "Any trucks free tomorrow?",
] as const;

/**
 * The chat transcript: user bubbles, assistant markdown, system notes, and per-turn tool-activity
 * timelines - interleaved chronologically. Owns its own scroll container, mirroring the copilot
 * drawer's pinned-to-bottom behavior scaled up to a full page.
 */
@Component({
  selector: "app-dispatch-transcript",
  templateUrl: "./dispatch-transcript.html",
  imports: [
    Alert,
    ChatMessage,
    DispatchTurnTimeline,
    EmptyState,
    Spinner,
    Stack,
    Typography,
    UiButton,
  ],
})
export class DispatchTranscript {
  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");

  protected readonly store = inject(DispatchChatStore);

  protected readonly suggestedPrompts = SUGGESTED_PROMPTS;

  protected readonly turnFailed = computed(() => this.store.turnStatus() === "failed");

  /** False while the user has scrolled up to read back - new items must not yank them down. */
  protected readonly pinnedToBottom = signal(true);

  protected readonly stream = computed<TranscriptItem[]>(() =>
    buildTranscriptStream(this.store.messages(), this.store.decisions(), this.store.sessions()),
  );

  constructor() {
    effect(() => {
      this.stream();
      this.store.isRunning();
      if (!this.pinnedToBottom()) return;
      const container = this.messagesContainer()?.nativeElement;
      if (container) {
        queueMicrotask(() => container.scrollTo({ top: container.scrollHeight }));
      }
    });
  }

  protected trackItem(item: TranscriptItem): string {
    return item.kind === "message" ? (item.message.id ?? "") : (item.session.id ?? "");
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
}
