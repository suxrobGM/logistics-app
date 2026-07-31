import { Component, input, output } from "@angular/core";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { UiButton } from "@logistics/shared/ui";

@Component({
  selector: "app-approve-reject-actions",
  templateUrl: "./approve-reject-actions.html",
  imports: [UiButton],
})
export class ApproveRejectActions {
  public readonly decision = input.required<AgentDecisionDto>();
  /** True while this decision's approve/reject request is in flight. */
  public readonly busy = input(false);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();
}
