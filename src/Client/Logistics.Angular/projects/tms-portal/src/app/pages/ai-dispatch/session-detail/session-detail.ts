import { DatePipe } from "@angular/common";
import {
  Component,
  type OnDestroy,
  type OnInit,
  computed,
  inject,
  input,
  signal,
} from "@angular/core";
import { PageHeader } from "@logistics/shared";
import {
  Api,
  type DispatchDecisionDto,
  type DispatchSessionDto,
  approveDispatchDecision,
  cancelDispatchSession,
  getDispatchSessionById,
  rejectDispatchDecision,
  replanDispatchSession,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TagModule } from "primeng/tag";
import { TimelineModule } from "primeng/timeline";
import { DispatchAgentHubService, TenantService, ToastService } from "@/core/services";
import { DateUtils, Labels } from "@/shared/utils";
import { ApproveRejectActions } from "../components/approve-reject-actions/approve-reject-actions";
import { ModeBadge } from "../components/mode-badge/mode-badge";
import { ToolOutputSummary } from "../components/tool-output-summary/tool-output-summary";
import {
  getToolIcon,
  getToolLabel,
  getToolMarkerClass,
  isWriteTool,
} from "../utils/decision-utils";
import { MarkdownPipe } from "../utils/markdown";

@Component({
  selector: "app-session-detail",
  templateUrl: "./session-detail.html",
  styleUrl: "./session-detail.css",
  imports: [
    ButtonModule,
    TagModule,
    TimelineModule,
    ConfirmDialogModule,
    DatePipe,
    PageHeader,
    MarkdownPipe,
    ModeBadge,
    ToolOutputSummary,
    ApproveRejectActions,
  ],
})
export class SessionDetailPage implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly dispatchAgentHub = inject(DispatchAgentHubService);
  private readonly tenantService = inject(TenantService);

  public readonly id = input.required<string>();

  protected readonly session = signal<DispatchSessionDto | null>(null);
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
      { label: "Started", value: datePipe.transform(s.startedAt, "medium") ?? "—" },
      {
        label: "Completed",
        value: s.completedAt ? datePipe.transform(s.completedAt, "medium") : "—",
      },
      { label: "Duration", value: this.duration() ?? "—" },
      { label: "Decisions", value: String(s.decisionCount ?? 0) },
      { label: "Model", value: s.modelUsed ?? "—", mono: true },
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

  ngOnDestroy(): void {
    const tenant = this.tenantService.getTenantData();
    if (tenant?.id) {
      this.dispatchAgentHub.unsubscribeFromDispatchBoard(tenant.id);
    }
  }

  private async setupSignalR(): Promise<void> {
    const tenant = this.tenantService.getTenantData();
    if (!tenant?.id) return;

    this.dispatchAgentHub.onReceiveDispatchAgentUpdate = (update) => {
      if (update.sessionId === this.id()) {
        this.loadSession();
      }
    };

    this.dispatchAgentHub.onReceiveDispatchDecision = (decision) => {
      if (decision.sessionId === this.id()) {
        this.session.update((s) => {
          if (!s) return s;
          const decisions = [...(s.decisions ?? []), decision];
          return { ...s, decisions, decisionCount: decisions.length };
        });
      }
    };

    await this.dispatchAgentHub.connect();
    await this.dispatchAgentHub.subscribeToDispatchBoard(tenant.id);
  }

  protected async loadSession(): Promise<void> {
    this.isLoading.set(true);
    try {
      const session = await this.api.invoke(getDispatchSessionById, { sessionId: this.id() });
      this.session.set(session);
    } catch {
      this.toastService.showError("Failed to load session");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async approveDecision(decision: DispatchDecisionDto): Promise<void> {
    try {
      await this.api.invoke(approveDispatchDecision, { decisionId: decision.id! });
      this.toastService.showSuccess("Decision approved and executed");
      await this.loadSession();
    } catch {
      this.toastService.showError("Failed to approve decision");
    }
  }

  protected rejectDecision(decision: DispatchDecisionDto): void {
    this.toastService.confirm({
      message: "Are you sure you want to reject this decision?",
      header: "Reject Decision",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await this.api.invoke(rejectDispatchDecision, { decisionId: decision.id!, body: {} });
          this.toastService.showSuccess("Decision rejected");
          await this.loadSession();
        } catch {
          this.toastService.showError("Failed to reject decision");
        }
      },
    });
  }

  protected async replanSession(): Promise<void> {
    const session = this.session();
    if (!session) return;

    try {
      await this.api.invoke(replanDispatchSession, {
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
      await this.api.invoke(cancelDispatchSession, { sessionId: session.id! });
      this.toastService.showSuccess("Session cancelled");
      await this.loadSession();
    } catch {
      this.toastService.showError("Failed to cancel session");
    }
  }
}
