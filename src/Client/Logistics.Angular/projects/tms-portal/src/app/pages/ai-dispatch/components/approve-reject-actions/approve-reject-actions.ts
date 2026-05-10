import { Component, input, output } from "@angular/core";
import type { AiDispatchDecisionDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";

@Component({
  selector: "app-approve-reject-actions",
  templateUrl: "./approve-reject-actions.html",
  imports: [ButtonModule],
})
export class ApproveRejectActions {
  public readonly decision = input.required<AiDispatchDecisionDto>();
  public readonly approve = output<AiDispatchDecisionDto>();
  public readonly reject = output<AiDispatchDecisionDto>();
}
