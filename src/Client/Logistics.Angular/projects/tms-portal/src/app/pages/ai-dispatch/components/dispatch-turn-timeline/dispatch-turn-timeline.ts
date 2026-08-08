import { DatePipe } from "@angular/common";
import { Component, computed, input, output, signal } from "@angular/core";
import { Permission, PermissionGuard } from "@logistics/shared";
import type { AgentDecisionDto, AgentSessionDto } from "@logistics/shared/api";
import {
  Badge,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiTimeline,
  UiTimelineContent,
  UiTimelineMarker,
} from "@logistics/shared/ui";
import { ApproveRejectActions, ToolOutputSummary } from "@/shared/components";
import { MarkdownPipe } from "@/shared/pipes";
import { getToolIcon, getToolLabel, getToolMarkerClass, isWriteTool, Labels } from "@/shared/utils";

/**
 * One agent turn's tool activity, ported from the old session-details "Agent Timeline": a
 * `ui-timeline` with a coloured marker per tool, expandable reasoning, tool-output summaries and an
 * inline approve/reject for suggested write-tool decisions.
 */
@Component({
  selector: "app-dispatch-turn-timeline",
  templateUrl: "./dispatch-turn-timeline.html",
  imports: [
    ApproveRejectActions,
    Badge,
    DatePipe,
    Icon,
    MarkdownPipe,
    PermissionGuard,
    Spinner,
    Stack,
    Surface,
    ToolOutputSummary,
    Typography,
    UiButton,
    UiTimeline,
    UiTimelineContent,
    UiTimelineMarker,
  ],
})
export class DispatchTurnTimeline {
  public readonly session = input.required<AgentSessionDto>();
  public readonly decisions = input.required<AgentDecisionDto[]>();
  /** Id of the decision whose approve/reject request is in flight. */
  public readonly busyDecisionId = input<string | null>(null);
  public readonly approve = output<AgentDecisionDto>();
  public readonly reject = output<AgentDecisionDto>();

  protected readonly Labels = Labels;
  protected readonly getToolLabel = getToolLabel;
  protected readonly getToolIcon = getToolIcon;
  protected readonly getToolMarkerClass = getToolMarkerClass;
  protected readonly isWriteTool = isWriteTool;
  protected readonly dispatchManage = Permission.Dispatch.Manage;

  /** Marks which past turn is the one currently in progress, alongside the transcript's own spinner. */
  protected readonly isTurnRunning = computed(() => this.session().status === "running");

  protected readonly expandedDecisions = signal<Set<string>>(new Set());

  protected toggleExpand(decisionId: string): void {
    this.expandedDecisions.update((set) => {
      const next = new Set(set);
      if (next.has(decisionId)) {
        next.delete(decisionId);
      } else {
        next.add(decisionId);
      }
      return next;
    });
  }

  protected isExpanded(decisionId: string): boolean {
    return this.expandedDecisions().has(decisionId);
  }
}
