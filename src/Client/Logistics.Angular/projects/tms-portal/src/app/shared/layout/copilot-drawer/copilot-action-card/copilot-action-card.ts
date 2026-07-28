import { Component, computed, input, output } from "@angular/core";
import { Permission, PermissionGuard } from "@logistics/shared";
import type { AIDispatchDecisionDto } from "@logistics/shared/api";
import { Badge, Icon } from "@logistics/shared/ui";
import { ApproveRejectActions } from "@/shared/components";
import { getToolIcon, getToolLabel, Labels } from "@/shared/utils";

/**
 * A suggested (or resolved) write action rendered inline in the chat stream. The approve/reject
 * buttons are permission-gated here; the API re-checks the tool's own permission on approval.
 */
@Component({
  selector: "app-copilot-action-card",
  templateUrl: "./copilot-action-card.html",
  imports: [ApproveRejectActions, Badge, Icon, PermissionGuard],
})
export class CopilotActionCard {
  public readonly decision = input.required<AIDispatchDecisionDto>();
  public readonly approve = output<AIDispatchDecisionDto>();
  public readonly reject = output<AIDispatchDecisionDto>();

  protected readonly Labels = Labels;
  protected readonly copilotManage = Permission.Copilot.Manage;
  protected readonly icon = computed(() => getToolIcon(this.decision().toolName));
  protected readonly label = computed(() => getToolLabel(this.decision().toolName));
  protected readonly isPending = computed(() => this.decision().status === "suggested");
}
