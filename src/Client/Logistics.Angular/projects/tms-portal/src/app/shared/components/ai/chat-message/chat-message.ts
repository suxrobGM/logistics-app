import { Component, input } from "@angular/core";
import type { AgentMessageDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";
import { MarkdownPipe } from "@/shared/pipes";

/**
 * One transcript row, shared by the copilot drawer and the AI dispatch chat page: assistant
 * messages render markdown, user messages plain text, system rows as a subtle centered note
 * (approval/rejection notices).
 */
@Component({
  selector: "app-chat-message",
  templateUrl: "./chat-message.html",
  imports: [Icon, MarkdownPipe],
  host: { class: "block" },
})
export class ChatMessage {
  public readonly message = input.required<AgentMessageDto>();
}
