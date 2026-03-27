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
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { TenantService, ToastService, TrackingService } from "@/core/services";
import { GeolocationMap } from "@/shared/components";
import { Labels } from "@/shared/utils";
import { DecisionCard } from "../components/decision-card/decision-card";
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
    GeolocationMap,
    DecisionCard,
  ],
})
export class SessionsListPage implements OnInit, OnDestroy {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly trackingService = inject(TrackingService);
  private readonly tenantService = inject(TenantService);

  protected readonly Labels = Labels;
  protected readonly Math = Math;
  protected readonly stripMarkdown = stripMarkdown;

  protected readonly sessions = signal<DispatchSessionDto[]>([]);
  protected readonly pendingDecisions = signal<DispatchDecisionDto[]>([]);
  protected readonly quotaStatus = signal<AiQuotaStatusDto | null>(null);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isRunning = signal(false);

  /** Only write-tool decisions (assign, create trip, dispatch) that need approval */
  protected readonly writeDecisions = computed(() =>
    this.pendingDecisions().filter((d) => d.type !== "query"),
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
    this.loadData();
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

    this.trackingService.onReceiveDispatchAgentUpdate = () => {
      this.loadData();
    };

    this.trackingService.onReceiveDispatchDecision = (decision) => {
      if (decision.status === "suggested") {
        this.pendingDecisions.update((list) => [...list, decision]);
      }
    };
  }

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [sessionsRes, pending, quota, trucksRes] = await Promise.all([
        this.api.invoke(getDispatchSessions, { Page: 1, PageSize: 20, OrderBy: "-StartedAt" }),
        this.api.invoke(getPendingDecisions),
        this.api.invoke(getAiQuotaStatus),
        this.api.invoke(getTrucks, { Status: "Available", PageSize: 100 }),
      ]);

      this.sessions.set(sessionsRes.items ?? []);
      this.pendingDecisions.set(pending ?? []);
      this.quotaStatus.set(quota);
      this.trucks.set(trucksRes.items ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async runAgent(mode: DispatchAgentMode): Promise<void> {
    this.isRunning.set(true);
    try {
      const sessionId = await this.api.invoke(runDispatchAgent, { body: { mode } });
      this.toastService.showSuccess("Agent session started");

      // Navigate to the session detail to watch progress
      if (sessionId) {
        this.router.navigate(["/ai-dispatch", sessionId]);
      } else {
        await this.loadData();
      }
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
          await this.api.invoke(rejectDispatchDecision, { decisionId: decision.id! });
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
