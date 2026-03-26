import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  type AiQuotaStatusDto,
  Api,
  type DispatchAgentMode,
  type DispatchDecisionDto,
  type DispatchSessionDto,
  approveDispatchDecision,
  getAiQuotaStatus,
  getDispatchSessions,
  getPendingDecisions,
  rejectDispatchDecision,
  runDispatchAgent,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { Labels } from "@/shared/utils";

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
    DecimalPipe,
  ],
})
export class SessionsListPage implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  protected readonly Labels = Labels;
  protected readonly Math = Math;
  protected readonly sessions = signal<DispatchSessionDto[]>([]);
  protected readonly pendingDecisions = signal<DispatchDecisionDto[]>([]);
  protected readonly quotaStatus = signal<AiQuotaStatusDto | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly isRunning = signal(false);

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [sessionsRes, pending, quota] = await Promise.all([
        this.api.invoke(getDispatchSessions, { Page: 1, PageSize: 20 }),
        this.api.invoke(getPendingDecisions),
        this.api.invoke(getAiQuotaStatus),
      ]);
      this.sessions.set(sessionsRes.items ?? []);
      this.pendingDecisions.set(pending ?? []);
      this.quotaStatus.set(quota);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async runAgent(mode: DispatchAgentMode): Promise<void> {
    this.isRunning.set(true);
    try {
      await this.api.invoke(runDispatchAgent, { body: { mode } });
      this.toastService.showSuccess("Agent session started");
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
