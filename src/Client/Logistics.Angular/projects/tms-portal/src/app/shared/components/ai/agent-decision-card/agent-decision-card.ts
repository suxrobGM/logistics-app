import { NgTemplateOutlet } from "@angular/common";
import { Component, computed, input, output } from "@angular/core";
import { PermissionGuard, type PermissionValue } from "@logistics/shared";
import type { AgentDecisionDto } from "@logistics/shared/api";
import { Badge, Icon, Surface } from "@logistics/shared/ui";
import { stripMarkdown } from "@/shared/pipes";
import {
  getDecisionRefs,
  getToolIcon,
  getToolLabel,
  isWriteDecision,
  Labels,
} from "@/shared/utils";
import { ApproveRejectActions } from "../approve-reject-actions/approve-reject-actions";
import { ToolOutputSummary } from "../tool-output-summary/tool-output-summary";

/**
 * `stream` renders a decision inline in a chat transcript - read tools collapse to a one-line
 * activity row, write tools open the full approval card. `compact` is the standalone card a side
 * panel lists pending writes with.
 */
type AgentDecisionCardVariant = "stream" | "compact";

/**
 * One agent decision, wherever it is shown. Approve/reject is permission-gated here for the UI's
 * sake only; the API re-checks on approval.
 */
@Component({
  selector: "app-agent-decision-card",
  templateUrl: "./agent-decision-card.html",
  imports: [
    ApproveRejectActions,
    Badge,
    Icon,
    NgTemplateOutlet,
    PermissionGuard,
    Surface,
    ToolOutputSummary,
  ],
})
export class AgentDecisionCard {
  public readonly decision = input.required<AgentDecisionDto>();
  /** Permission the host requires to act on its own decisions - dispatch and copilot differ. */
  public readonly managePermission = input.required<PermissionValue>();
  public readonly variant = input<AgentDecisionCardVariant>("stream");
  /** True while this decision's approve/reject request is in flight. */
  public readonly busy = input(false);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();

  protected readonly Labels = Labels;

  protected readonly icon = computed(() => getToolIcon(this.decision().toolName));
  protected readonly label = computed(() => getToolLabel(this.decision().toolName));
  protected readonly isPending = computed(() => this.decision().status === "suggested");
  protected readonly isWrite = computed(() => isWriteDecision(this.decision()));
  protected readonly summary = computed(() => stripMarkdown(this.decision().reasoning ?? ""));
  protected readonly refs = computed(() => getDecisionRefs(this.decision()));
}
