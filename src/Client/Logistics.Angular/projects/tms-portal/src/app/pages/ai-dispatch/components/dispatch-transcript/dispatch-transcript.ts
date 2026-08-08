import { Component, computed, inject, viewChild, type ElementRef } from "@angular/core";
import { Alert, Spinner, Stack, UiButton } from "@logistics/shared/ui";
import { ChatMessage, pinnedScroll, TurnStatus } from "@/shared/components";
import { DispatchChatStore } from "../../store/dispatch-chat.store";
import { DispatchReportMessage } from "../dispatch-report-message/dispatch-report-message";
import { DispatchTurnTimeline } from "../dispatch-turn-timeline/dispatch-turn-timeline";
import { DispatchWelcome } from "../dispatch-welcome/dispatch-welcome";
import { buildTranscriptStream, type TranscriptItem } from "./dispatch-transcript.utils";

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
    DispatchReportMessage,
    DispatchTurnTimeline,
    DispatchWelcome,
    Spinner,
    Stack,
    TurnStatus,
    UiButton,
  ],
})
export class DispatchTranscript {
  private readonly messagesContainer = viewChild<ElementRef<HTMLDivElement>>("messagesContainer");

  protected readonly store = inject(DispatchChatStore);

  protected readonly turnFailed = computed(() => this.store.turnStatus() === "failed");

  protected readonly stream = computed<TranscriptItem[]>(() =>
    buildTranscriptStream(this.store.messages(), this.store.decisions(), this.store.sessions()),
  );

  protected readonly scroll = pinnedScroll(this.messagesContainer, () => {
    this.stream();
    this.store.isRunning();
  });

  protected trackItem(item: TranscriptItem): string {
    return item.kind === "message" ? (item.message.id ?? "") : (item.session.id ?? "");
  }
}
