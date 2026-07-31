import { Component, computed, input, output } from "@angular/core";
import { Permission, PermissionGuard } from "@logistics/shared";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { Badge, Icon } from "@logistics/shared/ui";
import { ApproveRejectActions, ToolOutputSummary } from "@/shared/components";
import { getDecisionRefs, getToolIcon, getToolLabel, isWriteTool, Labels } from "@/shared/utils";

/**
 * One agent decision in the stream: read tools render as a compact activity row, write tools as
 * the full approval card. Approve/reject is permission-gated here; the API re-checks on approval.
 */
@Component({
  selector: "app-copilot-action-card",
  templateUrl: "./copilot-action-card.html",
  imports: [ApproveRejectActions, Badge, Icon, PermissionGuard, ToolOutputSummary],
})
export class CopilotActionCard {
  public readonly decision = input.required<AgentDecisionDto>();
  /** True while this decision's approve/reject request is in flight. */
  public readonly busy = input(false);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();

  protected readonly Labels = Labels;
  protected readonly copilotManage = Permission.Copilot.Manage;
  protected readonly icon = computed(() => getToolIcon(this.decision().toolName));
  protected readonly label = computed(() => getToolLabel(this.decision().toolName));
  protected readonly isPending = computed(() => this.decision().status === "suggested");
  protected readonly isWrite = computed(() => isWriteTool(this.decision().toolName));
  protected readonly refs = computed(() => getDecisionRefs(this.decision()));
}
