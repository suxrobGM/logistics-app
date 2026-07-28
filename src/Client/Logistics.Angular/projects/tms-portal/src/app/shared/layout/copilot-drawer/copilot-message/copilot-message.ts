import { Component, input } from "@angular/core";
import type { AICopilotMessageDto } from "@logistics/shared/api";
import { Icon } from "@logistics/shared/ui";
import { MarkdownPipe } from "@/shared/pipes";

/** Assistant messages render markdown, user messages plain text, system rows as muted notes. */
@Component({
  selector: "app-copilot-message",
  templateUrl: "./copilot-message.html",
  imports: [Icon, MarkdownPipe],
})
export class CopilotMessage {
  public readonly message = input.required<AICopilotMessageDto>();
}
