import { Component, computed, input, output } from "@angular/core";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { Badge, Icon, Stack, Surface, Typography, UiButton } from "@logistics/shared/ui";
import { stripMarkdown } from "@/shared/pipes";
import { getDecisionRefs, getToolLabel, Labels } from "@/shared/utils";

/** A compact, tenant-wide pending write-decision card for the dispatch page's right panel. */
@Component({
  selector: "app-decision-card",
  templateUrl: "./decision-card.html",
  imports: [Badge, Icon, Stack, Surface, Typography, UiButton],
})
export class DecisionCard {
  public readonly decision = input.required<AgentDecisionDto>();
  /** True while this decision's approve/reject request is in flight. */
  public readonly busy = input(false);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();

  protected readonly Labels = Labels;
  protected readonly stripMarkdown = stripMarkdown;

  protected readonly toolLabel = computed(() => getToolLabel(this.decision().toolName));
  protected readonly refs = computed(() => getDecisionRefs(this.decision()));

  protected onApprove(): void {
    this.approve.emit(this.decision());
  }

  protected onReject(): void {
    this.reject.emit(this.decision());
  }
}
