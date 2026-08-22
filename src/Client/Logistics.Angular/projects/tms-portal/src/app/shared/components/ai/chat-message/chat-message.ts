import { Component, computed, input } from "@angular/core";
import type { AgentMessageDto } from "@logistics/shared/api";
import { Icon, Markdown } from "@logistics/shared/ui";
import { senderLabel } from "./chat-message.utils";

/**
 * One transcript row, shared by the copilot drawer and the AI dispatch chat page: assistant
 * messages render markdown, user messages plain text, system rows as a subtle centered note
 * (approval/rejection notices).
 */
@Component({
  selector: "app-chat-message",
  templateUrl: "./chat-message.html",
  imports: [Icon, Markdown],
  host: { class: "block" },
})
export class ChatMessage {
  public readonly message = input.required<AgentMessageDto>();

  /** Who is reading. A dispatch conversation is shared, so their own rows read "You". */
  public readonly currentUserId = input<string | null>(null);

  protected readonly sender = computed(() => senderLabel(this.message(), this.currentUserId()));
}
