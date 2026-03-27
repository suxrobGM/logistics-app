import { DatePipe, DecimalPipe } from "@angular/common";
import {
  Component,
  type OnDestroy,
  type OnInit,
  computed,
  inject,
  input,
  signal,
} from "@angular/core";
import { Router } from "@angular/router";
import { PageHeader } from "@logistics/shared";
import {
  Api,
  type DispatchDecisionDto,
  type DispatchSessionDto,
  approveDispatchDecision,
  cancelDispatchSession,
  getDispatchSessionById,
  rejectDispatchDecision,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TagModule } from "primeng/tag";
import { TimelineModule } from "primeng/timeline";
import { TooltipModule } from "primeng/tooltip";
import { TenantService, ToastService, TrackingService } from "@/core/services";
import { DateUtils, Labels } from "@/shared/utils";
import {
  getToolIcon,
  getToolLabel,
  getToolMarkerClass,
  isWriteTool,
  parseToolOutput,
} from "../utils/decision-utils";
import { MarkdownPipe } from "../utils/markdown";

@Component({
  selector: "app-session-detail",
  templateUrl: "./session-detail.html",
  imports: [
    ButtonModule,
    TagModule,
    TooltipModule,
    TimelineModule,
    ConfirmDialogModule,
    DatePipe,
    DecimalPipe,
    PageHeader,
    MarkdownPipe,
  ],
})
export class SessionDetailPage implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly trackingService = inject(TrackingService);
  private readonly tenantService = inject(TenantService);

  public readonly id = input.required<string>();

  protected readonly session = signal<DispatchSessionDto | null>(null);
  protected readonly isLoading = signal(false);

  protected readonly Labels = Labels;
  protected readonly getToolLabel = getToolLabel;
  protected readonly getToolIcon = getToolIcon;
  protected readonly getToolMarkerClass = getToolMarkerClass;
  protected readonly isWriteTool = isWriteTool;
  protected readonly parseToolOutput = parseToolOutput;

  /** Track which decision IDs have expanded reasoning */
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

  protected readonly duration = computed(() => {
    const s = this.session();
    return DateUtils.formatDuration(s?.startedAt, s?.completedAt);
  });

  ngOnInit(): void {
    this.loadSession();
    this.setupSignalR();
  }

  ngOnDestroy(): void {
    const tenant = this.tenantService.getTenantData();
    if (tenant?.id) {
      this.trackingService.unsubscribeFromDispatchBoard(tenant.id);
    }
  }

  private async setupSignalR(): Promise<void> {
    const tenant = this.tenantService.getTenantData();
    if (!tenant?.id) return;

    await this.trackingService.connect();
    await this.trackingService.subscribeToDispatchBoard(tenant.id);

    this.trackingService.onReceiveDispatchAgentUpdate = (update) => {
      if (update.sessionId === this.id()) {
        this.loadSession();
      }
    };

    this.trackingService.onReceiveDispatchDecision = (decision) => {
      if (decision.sessionId === this.id()) {
        // Add the new decision to the timeline in real-time
        this.session.update((s) => {
          if (!s) return s;
          const decisions = [...(s.decisions ?? []), decision];
          return { ...s, decisions, decisionCount: decisions.length };
        });
      }
    };
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
          await this.api.invoke(rejectDispatchDecision, { decisionId: decision.id! });
          this.toastService.showSuccess("Decision rejected");
          await this.loadSession();
        } catch {
          this.toastService.showError("Failed to reject decision");
        }
      },
    });
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

  protected goBack(): void {
    this.router.navigate(["/ai-dispatch"]);
  }
}
