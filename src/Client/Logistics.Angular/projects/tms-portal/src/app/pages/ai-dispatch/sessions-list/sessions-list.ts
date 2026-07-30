import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnDestroy, type OnInit } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { PageHeader } from "@logistics/shared";
import {
  Api,
  getAIDispatchSessions,
  getAIQuotaStatus,
  getPendingDecisions,
  getTrucks,
  runAIDispatch,
  type AgentAutonomyMode,
  type AgentDecisionDto,
  type AgentSessionDto,
  type AIQuotaStatusDto,
  type TruckDto,
} from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import type { ListLazyLoadEvent } from "@logistics/shared/stores";
import {
  Badge,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDataTable,
  UiTooltip,
} from "@logistics/shared/ui";
import {
  AIDispatchHubService,
  DispatchBadgeService,
  TenantService,
  ToastService,
} from "@/core/services";
import {
  AIQuotaUsage,
  DecisionActionsService,
  GeolocationMap,
  RejectDecisionDialog,
} from "@/shared/components";
import { stripMarkdown } from "@/shared/pipes";
import { Labels } from "@/shared/utils";
import { DecisionCard } from "../components/decision-card/decision-card";
import { ModeBadge } from "../components/mode-badge/mode-badge";
import {
  RunAgentDialog,
  type RunAgentDialogData,
} from "../components/run-agent-dialog/run-agent-dialog";

@Component({
  selector: "app-sessions-list",
  templateUrl: "./sessions-list.html",
  providers: [DecisionActionsService],
  imports: [
    AIQuotaUsage,
    Badge,
    DatePipe,
    DecisionCard,
    GeolocationMap,
    Icon,
    ModeBadge,
    PageHeader,
    RejectDecisionDialog,
    RouterLink,
    RunAgentDialog,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class SessionsListPage implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly aiDispatchHub = inject(AIDispatchHubService);
  private readonly tenantService = inject(TenantService);
  private readonly dispatchBadgeService = inject(DispatchBadgeService);
  protected readonly decisionActions = inject(DecisionActionsService);

  protected readonly Labels = Labels;
  protected readonly Math = Math;
  protected readonly stripMarkdown = stripMarkdown;

  protected readonly sessions = signal<AgentSessionDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly first = signal(0);
  protected readonly pendingDecisions = signal<AgentDecisionDto[]>([]);
  protected readonly quotaStatus = signal<AIQuotaStatusDto | null>(null);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isRunning = signal(false);
  protected readonly showRunDialog = signal(false);
  protected readonly runMode = signal<AgentAutonomyMode>("human_in_the_loop");

  /** Only write-tool decisions (assign, create trip, dispatch) that need approval */
  protected readonly writeDecisions = computed(() =>
    this.pendingDecisions().filter((d) => d.type !== "query"),
  );

  /** Disable run buttons when a session is already running */
  protected readonly hasRunningSession = computed(() =>
    this.sessions().some((s) => s.status === "running"),
  );

  protected readonly truckLocations = computed<TruckGeolocationDto[]>(() => {
    return this.trucks()
      .filter((t) => t.currentLocation?.latitude && t.currentLocation?.longitude)
      .map((t) => ({
        truckId: t.id,
        truckNumber: t.number,
        driversName: [t.mainDriver?.fullName, t.secondaryDriver?.fullName]
          .filter(Boolean)
          .join(", "),
        currentLocation: t.currentLocation,
        currentAddress: t.currentAddress,
      }));
  });

  ngOnInit(): void {
    this.setupSignalR();
  }

  ngOnDestroy(): void {
    const tenant = this.tenantService.getTenantData();
    if (tenant?.id) {
      this.aiDispatchHub.unsubscribeFromDispatchBoard(tenant.id);
    }
  }

  private async setupSignalR(): Promise<void> {
    const tenant = this.tenantService.getTenantData();
    if (!tenant?.id) return;

    this.aiDispatchHub.onReceiveAIDispatchUpdate = () => {
      this.loadData();
    };

    this.aiDispatchHub.onReceiveAIDispatchDecision = (decision) => {
      if (decision.status === "suggested") {
        this.pendingDecisions.update((list) => [...list, decision]);
      }
    };

    await this.aiDispatchHub.connect();
    await this.aiDispatchHub.subscribeToDispatchBoard(tenant.id);
  }

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [sessionsRes, pending, quota, trucksRes] = await Promise.all([
        this.api.invoke(getAIDispatchSessions, {
          Page: this.page(),
          PageSize: this.pageSize(),
          OrderBy: "-StartedAt",
        }),
        this.api.invoke(getPendingDecisions),
        this.api.invoke(getAIQuotaStatus),
        this.api.invoke(getTrucks, { Status: "Available", PageSize: 100 }),
      ]);

      this.sessions.set(sessionsRes.items ?? []);
      this.totalRecords.set(sessionsRes.pagination?.total ?? 0);
      this.pendingDecisions.set(pending ?? []);
      this.quotaStatus.set(quota);
      this.trucks.set(trucksRes.items ?? []);
      this.dispatchBadgeService.pendingCount.set(this.writeDecisions().length);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected onPageChange(event: ListLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize();
    this.page.set(Math.floor(first / rows) + 1);
    this.pageSize.set(rows);
    this.first.set(first);
    this.loadData();
  }

  protected openRunDialog(mode: AgentAutonomyMode): void {
    this.runMode.set(mode);
    this.showRunDialog.set(true);
  }

  protected async onRunConfirmed(event: RunAgentDialogData): Promise<void> {
    this.isRunning.set(true);
    try {
      await this.api.invoke(runAIDispatch, {
        body: { mode: event.mode, instructions: event.instructions },
      });
      this.toastService.showSuccess("Agent session started - updates will appear in real-time");
      await this.loadData();
    } catch {
      this.toastService.showError("Failed to start agent session");
    } finally {
      this.isRunning.set(false);
    }
  }

  protected approveDecision(decision: AgentDecisionDto): void {
    this.decisionActions.approve(decision, () => this.loadData());
  }

  protected rejectDecision(decision: AgentDecisionDto): void {
    this.decisionActions.reject(decision, () => this.loadData());
  }

  protected viewSession(session: AgentSessionDto): void {
    this.router.navigate(["/ai-dispatch", session.id]);
  }
}
