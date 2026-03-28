import { Component, computed, input, output, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { DispatchDecisionDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { Labels } from "@/shared/utils";
import { getToolLabel, parseToolInput } from "../../utils/decision-utils";
import { stripMarkdown } from "../../utils/markdown";

@Component({
  selector: "app-decision-card",
  templateUrl: "./decision-card.html",
  imports: [ButtonModule, TagModule, TooltipModule, RouterLink],
})
export class DecisionCard {
  public readonly decision = input.required<DispatchDecisionDto>();
  public readonly showSessionLink = input(false);
  public readonly approve = output<DispatchDecisionDto>();
  public readonly reject = output<DispatchDecisionDto>();

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
