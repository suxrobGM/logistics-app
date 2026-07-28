import { Component, input } from "@angular/core";
import type { AICopilotMessageDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";
import { MarkdownPipe } from "@/shared/pipes";

/**
 * One chat bubble. Assistant messages render markdown; user messages are plain text;
 * system rows (approval outcomes) render as muted notes.
 */
@Component({
  selector: "app-copilot-message",
  templateUrl: "./copilot-message.html",
  imports: [Icon, MarkdownPipe],
})
export class CopilotMessage {
  public readonly message = input.required<AICopilotMessageDto>();
}
