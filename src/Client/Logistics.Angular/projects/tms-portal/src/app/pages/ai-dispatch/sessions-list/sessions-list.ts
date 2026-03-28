import { DatePipe } from "@angular/common";
import { Component, type OnDestroy, type OnInit, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { PageHeader } from "@logistics/shared";
import {
  type AiQuotaStatusDto,
  Api,
  type DispatchAgentMode,
  type DispatchDecisionDto,
  type DispatchSessionDto,
  type TruckDto,
  approveDispatchDecision,
  getAiQuotaStatus,
  getDispatchSessions,
  getPendingDecisions,
  getTrucks,
  rejectDispatchDecision,
  runDispatchAgent,
} from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import type { TableLazyLoadEvent } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import {
  DispatchAgentHubService,
  DispatchBadgeService,
  TenantService,
  ToastService,
} from "@/core/services";
import { AiQuotaUsage, GeolocationMap } from "@/shared/components";
import { Labels } from "@/shared/utils";
import { DecisionCard } from "../components/decision-card/decision-card";
import { ModeBadge } from "../components/mode-badge/mode-badge";
import {
  RunAgentDialog,
  type RunAgentDialogData,
} from "../components/run-agent-dialog/run-agent-dialog";
import { stripMarkdown } from "../utils/markdown";

@Component({
  selector: "app-sessions-list",
  templateUrl: "./sessions-list.html",
  imports: [
    ButtonModule,
    TableModule,
    TagModule,
    TooltipModule,
    ConfirmDialogModule,
    DatePipe,
    PageHeader,
    AiQuotaUsage,
    GeolocationMap,
    DecisionCard,
    ModeBadge,
    RunAgentDialog,
  ],
})
export class SessionsListPage implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly dispatchAgentHub = inject(DispatchAgentHubService);
  private readonly tenantService = inject(TenantService);
  private readonly dispatchBadgeService = inject(DispatchBadgeService);

  protected readonly Labels = Labels;
  protected readonly Math = Math;
  protected readonly stripMarkdown = stripMarkdown;

  protected readonly sessions = signal<DispatchSessionDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly page = signal(1);
  protected readonly pageSize = signal(10);
  protected readonly first = signal(0);
  protected readonly pendingDecisions = signal<DispatchDecisionDto[]>([]);
  protected readonly quotaStatus = signal<AiQuotaStatusDto | null>(null);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isRunning = signal(false);
  protected readonly showRunDialog = signal(false);
  protected readonly runMode = signal<DispatchAgentMode>("human_in_the_loop");

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
      this.dispatchAgentHub.unsubscribeFromDispatchBoard(tenant.id);
    }
  }

  private async setupSignalR(): Promise<void> {
    const tenant = this.tenantService.getTenantData();
    if (!tenant?.id) return;

    this.dispatchAgentHub.onReceiveDispatchAgentUpdate = () => {
      this.loadData();
    };

    this.dispatchAgentHub.onReceiveDispatchDecision = (decision) => {
      if (decision.status === "suggested") {
        this.pendingDecisions.update((list) => [...list, decision]);
      }
    };

    await this.dispatchAgentHub.connect();
    await this.dispatchAgentHub.subscribeToDispatchBoard(tenant.id);
  }

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [sessionsRes, pending, quota, trucksRes] = await Promise.all([
        this.api.invoke(getDispatchSessions, {
          Page: this.page(),
          PageSize: this.pageSize(),
          OrderBy: "-StartedAt",
        }),
        this.api.invoke(getPendingDecisions),
        this.api.invoke(getAiQuotaStatus),
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

  protected onPageChange(event: TableLazyLoadEvent): void {
    const first = event.first ?? 0;
    const rows = event.rows ?? this.pageSize();
    this.page.set(Math.floor(first / rows) + 1);
    this.pageSize.set(rows);
    this.first.set(first);
    this.loadData();
  }

  protected openRunDialog(mode: DispatchAgentMode): void {
    this.runMode.set(mode);
    this.showRunDialog.set(true);
  }

  protected async onRunConfirmed(event: RunAgentDialogData): Promise<void> {
    this.isRunning.set(true);
    try {
      await this.api.invoke(runDispatchAgent, {
        body: { mode: event.mode, instructions: event.instructions },
      });
      this.toastService.showSuccess("Agent session started — updates will appear in real-time");
      await this.loadData();
    } catch {
      this.toastService.showError("Failed to start agent session");
    } finally {
      this.isRunning.set(false);
    }
  }

  protected async approveDecision(decision: DispatchDecisionDto): Promise<void> {
    try {
      await this.api.invoke(approveDispatchDecision, { decisionId: decision.id! });
      this.toastService.showSuccess("Decision approved and executed");
      await this.loadData();
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
          await this.loadData();
        } catch {
          this.toastService.showError("Failed to reject decision");
        }
      },
    });
  }

  protected viewSession(session: DispatchSessionDto): void {
    this.router.navigate(["/ai-dispatch", session.id]);
  }
}
