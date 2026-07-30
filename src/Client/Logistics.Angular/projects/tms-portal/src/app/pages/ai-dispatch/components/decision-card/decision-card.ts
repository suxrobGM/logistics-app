import { Component, computed, input, output, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { Badge, Icon, Stack, Surface, UiButton } from "@logistics/shared/ui";
import { stripMarkdown } from "@/shared/pipes";
import { getToolLabel, Labels, parseToolInput } from "@/shared/utils";

@Component({
  selector: "app-decision-card",
  templateUrl: "./decision-card.html",
  imports: [Badge, Icon, RouterLink, Stack, Surface, UiButton],
})
export class DecisionCard {
  public readonly decision = input.required<AgentDecisionDto>();
  public readonly showSessionLink = input(false);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();

  protected readonly Labels = Labels;
  protected readonly stripMarkdown = stripMarkdown;
  protected readonly isApproving = signal(false);

  protected readonly toolLabel = computed(() => getToolLabel(this.decision().toolName));
  protected readonly parsedInput = computed(() => parseToolInput(this.decision().toolInput));

  protected onApprove(): void {
    this.approve.emit(this.decision());
  }

  protected onReject(): void {
    this.reject.emit(this.decision());
  }
}
