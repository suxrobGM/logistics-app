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
  type IconName,
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
import { groupTurnEntries, type TurnEntry } from "./dispatch-turn-timeline.utils";

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
  protected readonly dispatchManage = Permission.Dispatch.Manage;

  /** Marks which past turn is the one currently in progress, alongside the transcript's own spinner. */
  protected readonly isTurnRunning = computed(() => this.session().status === "running");

  protected readonly timeline = computed(() => groupTurnEntries(this.decisions()));

  // A reads group's id is its first decision's id, so the two disclosures need distinct keys.
  private readonly expanded = signal<ReadonlySet<string>>(new Set());

  protected markerFor(entry: TurnEntry): { class: string; icon: IconName } {
    return entry.kind === "write"
      ? { class: getToolMarkerClass(entry.decision), icon: getToolIcon(entry.decision.toolName) }
      : { class: "bg-muted text-muted-foreground", icon: "search" };
  }

  protected toggle(key: string): void {
    this.expanded.update((set) => {
      const next = new Set(set);
      if (!next.delete(key)) next.add(key);
      return next;
    });
  }

  protected isExpanded(key: string): boolean {
    return this.expanded().has(key);
  }

  protected approve(decision: AgentDecisionDto): void {
    this.actions.approve(decision, () => this.store.onDecisionResolved());
  }

  protected reject(decision: AgentDecisionDto): void {
    this.actions.reject(decision, () => this.store.onDecisionResolved());
  }
}
