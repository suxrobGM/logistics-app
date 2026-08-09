import { DecimalPipe } from "@angular/common";
import { Component, input } from "@angular/core";
import type { AgentMessageDto, AgentSessionDto } from "@logistics/shared/api";
import { Icon, Surface } from "@logistics/shared/ui";
import { Markdown } from "@/shared/components";

/** A turn's closing assistant message, rendered as the dispatch report card. */
@Component({
  selector: "app-dispatch-report-message",
  templateUrl: "./dispatch-report-message.html",
  imports: [DecimalPipe, Icon, Markdown, Surface],
  host: { class: "block" },
})
export class DispatchReportMessage {
  public readonly message = input.required<AgentMessageDto>();
  public readonly session = input<AgentSessionDto>();
}
