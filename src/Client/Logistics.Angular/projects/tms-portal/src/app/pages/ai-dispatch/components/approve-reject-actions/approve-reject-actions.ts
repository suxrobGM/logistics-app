import { Component, input, output } from "@angular/core";
import type { AIDispatchDecisionDto } from "@logistics/shared/api";
import { UiButton } from "@logistics/shared/ui";

@Component({
  selector: "app-approve-reject-actions",
  templateUrl: "./approve-reject-actions.html",
  imports: [UiButton],
})
export class ApproveRejectActions {
  public readonly decision = input.required<AIDispatchDecisionDto>();
  public readonly approve = output<AIDispatchDecisionDto>();
  public readonly reject = output<AIDispatchDecisionDto>();
}
