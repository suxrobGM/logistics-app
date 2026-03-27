import { Component, input, output } from "@angular/core";
import type { DispatchDecisionDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "app-approve-reject-actions",
  templateUrl: "./approve-reject-actions.html",
  imports: [ButtonModule],
})
export class ApproveRejectActions {
  public readonly decision = input.required<DispatchDecisionDto>();
  public readonly approve = output<DispatchDecisionDto>();
  public readonly reject = output<DispatchDecisionDto>();
}
