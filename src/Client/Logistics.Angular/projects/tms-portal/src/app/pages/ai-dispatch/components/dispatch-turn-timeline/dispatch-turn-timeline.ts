import { DatePipe, NgTemplateOutlet } from "@angular/common";
import { Component, computed, inject, input, signal } from "@angular/core";
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
import {
  ApproveRejectActions,
  DecisionActionsService,
  ToolOutputSummary,
} from "@/shared/components";
import { MarkdownPipe } from "@/shared/pipes";
import { getToolIcon, getToolLabel, getToolMarkerClass, Labels } from "@/shared/utils";
import { DispatchChatStore } from "../../store/dispatch-chat.store";
import { ToolResultDetails } from "../tool-result-details/tool-result-details";
import {
  groupTurnEntries,
  readGroupSummary,
  type ReadGroupSummary,
} from "./dispatch-turn-timeline.utils";

/**
 * One agent turn's tool activity: a `ui-timeline` where consecutive read tools collapse into a
 * single "Checked:" chip row and each write tool keeps a prominent block with approve/reject.
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
    NgTemplateOutlet,
    PermissionGuard,
    Spinner,
    Stack,
    Surface,
    ToolOutputSummary,
    ToolResultDetails,
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

  private readonly store = inject(DispatchChatStore);
  protected readonly actions = inject(DecisionActionsService);

  protected readonly Labels = Labels;
  protected readonly getToolLabel = getToolLabel;
  protected readonly getToolIcon = getToolIcon;
  protected readonly getToolMarkerClass = getToolMarkerClass;
  protected readonly dispatchManage = Permission.Dispatch.Manage;

  /** Marks which past turn is the one currently in progress, alongside the transcript's own spinner. */
  protected readonly isTurnRunning = computed(() => this.session().status === "running");

  protected readonly timeline = computed(() => groupTurnEntries(this.decisions()));

  protected readonly readSummaries = computed(() => {
    const summaries = new Map<string, ReadGroupSummary>();
    for (const entry of this.timeline()) {
      if (entry.kind === "reads") summaries.set(entry.id, readGroupSummary(entry.decisions));
    }
    return summaries;
  });

  protected readonly expandedGroups = signal<ReadonlySet<string>>(new Set());
  protected readonly expandedDecisions = signal<ReadonlySet<string>>(new Set());

  protected toggleGroup(groupId: string): void {
    this.expandedGroups.update((set) => toggled(set, groupId));
  }

  protected isGroupExpanded(groupId: string): boolean {
    return this.expandedGroups().has(groupId);
  }

  protected toggleExpand(decisionId: string): void {
    this.expandedDecisions.update((set) => toggled(set, decisionId));
  }

  protected isExpanded(decisionId: string): boolean {
    return this.expandedDecisions().has(decisionId);
  }

  protected approve(decision: AgentDecisionDto): void {
    this.actions.approve(decision, () => this.store.onDecisionResolved());
  }

  protected reject(decision: AgentDecisionDto): void {
    this.actions.reject(decision, () => this.store.onDecisionResolved());
  }
}

function toggled(set: ReadonlySet<string>, id: string): ReadonlySet<string> {
  const next = new Set(set);
  if (!next.delete(id)) next.add(id);
  return next;
}
