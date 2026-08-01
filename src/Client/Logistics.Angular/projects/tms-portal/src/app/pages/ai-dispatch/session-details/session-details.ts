import { DatePipe } from "@angular/common";
import { Component, computed, DestroyRef, inject, input, signal, type OnInit } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { PageHeader } from "@logistics/shared";
import {
  Api,
  cancelAIDispatchSession,
  getAIDispatchSessionById,
  replanAIDispatchSession,
  type AgentDecisionDto,
  type AgentSessionDto,
} from "@logistics/shared/api";
import {
  Badge,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiTimeline,
  UiTimelineContent,
  UiTimelineMarker,
  UiTooltip,
} from "@logistics/shared/ui";
import { AIDispatchHubService, ToastService } from "@/core/services";
import {
  ApproveRejectActions,
  DecisionActionsService,
  RejectDecisionDialog,
  ToolOutputSummary,
} from "@/shared/components";
import { MarkdownPipe } from "@/shared/pipes";
import {
  DateUtils,
  getToolIcon,
  getToolLabel,
  getToolMarkerClass,
  isWriteTool,
  Labels,
} from "@/shared/utils";
import { ModeBadge } from "../components/mode-badge/mode-badge";

@Component({
  selector: "app-session-details",
  templateUrl: "./session-details.html",
  styleUrl: "./session-details.css",
  providers: [DecisionActionsService],
  imports: [
    ApproveRejectActions,
    Badge,
    DatePipe,
    Grid,
    Icon,
    MarkdownPipe,
    ModeBadge,
    PageHeader,
    RejectDecisionDialog,
    Stack,
    Surface,
    ToolOutputSummary,
    Typography,
    UiButton,
    UiTimeline,
    UiTimelineContent,
    UiTimelineMarker,
    UiTooltip,
  ],
})
export class SessionDetailsPage implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly aiDispatchHub = inject(AIDispatchHubService);
  private readonly destroyRef = inject(DestroyRef);
  protected readonly decisionActions = inject(DecisionActionsService);

  public readonly id = input.required<string>();

  protected readonly session = signal<AgentSessionDto | null>(null);
  protected readonly isLoading = signal(false);

  protected readonly Labels = Labels;
  protected readonly getToolLabel = getToolLabel;
  protected readonly getToolIcon = getToolIcon;
  protected readonly getToolMarkerClass = getToolMarkerClass;
  protected readonly isWriteTool = isWriteTool;

  protected readonly expandedDecisions = signal<Set<string>>(new Set());

  protected readonly duration = computed(() => {
    const s = this.session();
    return DateUtils.formatDuration(s?.startedAt, s?.completedAt);
  });

  protected readonly hasRejectedDecisions = computed(() => {
    const s = this.session();
    return s?.decisions?.some((d) => d.status === "rejected") ?? false;
  });

  protected readonly statsItems = computed(() => {
    const s = this.session();
    if (!s) return [];
    const datePipe = new DatePipe("en-US");
    const items = [
      { label: "Started", value: datePipe.transform(s.startedAt, "medium") ?? "-" },
      {
        label: "Completed",
        value: s.completedAt ? datePipe.transform(s.completedAt, "medium") : "-",
      },
      { label: "Duration", value: this.duration() ?? "-" },
      { label: "Decisions", value: String(s.decisionCount ?? 0) },
      { label: "Tokens Used", value: (s.totalTokensUsed ?? 0).toLocaleString(), mono: true },
    ];
    return items;
  });

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

  ngOnInit(): void {
    this.loadSession();
    this.setupSignalR();
  }

  private setupSignalR(): void {
    this.aiDispatchHub.updateReceived$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((update) => {
        if (update.sessionId === this.id()) {
          this.loadSession();
        }
      });

    this.aiDispatchHub.decisionReceived$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((decision) => {
        if (decision.sessionId === this.id()) {
          this.session.update((s) => {
            if (!s) return s;
            const decisions = [...(s.decisions ?? []), decision];
            return { ...s, decisions, decisionCount: decisions.length };
          });
        }
      });

    void this.aiDispatchHub.acquireDispatchBoard(this.destroyRef);
  }

  protected async loadSession(): Promise<void> {
    this.isLoading.set(true);
    try {
      const session = await this.api.invoke(getAIDispatchSessionById, { sessionId: this.id() });
      this.session.set(session);
    } catch {
      this.toastService.showError("Failed to load session");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected approveDecision(decision: AgentDecisionDto): void {
    this.decisionActions.approve(decision, () => this.loadSession());
  }

  protected rejectDecision(decision: AgentDecisionDto): void {
    this.decisionActions.reject(decision, () => this.loadSession());
  }

  protected async replanSession(): Promise<void> {
    const session = this.session();
    if (!session) return;

    try {
      await this.api.invoke(replanAIDispatchSession, {
        sessionId: session.id!,
        body: {},
      });
      this.toastService.showSuccess("Re-plan session started with rejection context");
    } catch {
      this.toastService.showError("Failed to start re-plan session");
    }
  }

  protected async cancelSession(): Promise<void> {
    const session = this.session();
    if (!session) return;

    try {
      await this.api.invoke(cancelAIDispatchSession, { sessionId: session.id! });
      this.toastService.showSuccess("Session cancelled");
      await this.loadSession();
    } catch {
      this.toastService.showError("Failed to cancel session");
    }
  }
}
