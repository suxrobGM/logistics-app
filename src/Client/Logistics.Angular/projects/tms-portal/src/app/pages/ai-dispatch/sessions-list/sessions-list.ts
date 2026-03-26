import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type {
  DispatchAgentMode,
  DispatchDecisionDto,
  DispatchSessionDto,
} from "@logistics/shared/api";
import { ConfirmationService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DispatchAgentService } from "../store/dispatch-agent.service";

@Component({
  selector: "app-sessions-list",
  templateUrl: "./sessions-list.html",
  providers: [ConfirmationService],
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
  private readonly agentService = inject(DispatchAgentService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly confirmService = inject(ConfirmationService);

  protected readonly sessions = signal<DispatchSessionDto[]>([]);
  protected readonly pendingDecisions = signal<DispatchDecisionDto[]>([]);
  protected readonly isLoading = signal(false);
  protected readonly isRunning = signal(false);

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [sessionsRes, pending] = await Promise.all([
        this.agentService.getSessions(1, 20),
        this.agentService.getPendingDecisions(),
      ]);
      this.sessions.set(sessionsRes.items ?? []);
      this.pendingDecisions.set(pending ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async runAgent(mode: DispatchAgentMode): Promise<void> {
    this.isRunning.set(true);
    try {
      await this.agentService.run(mode);
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
      await this.agentService.approveDecision(decision.id!);
      this.toastService.showSuccess("Decision approved and executed");
      await this.loadData();
    } catch {
      this.toastService.showError("Failed to approve decision");
    }
  }

  protected rejectDecision(decision: DispatchDecisionDto): void {
    this.confirmService.confirm({
      message: "Are you sure you want to reject this decision?",
      header: "Reject Decision",
      icon: "pi pi-exclamation-triangle",
      accept: async () => {
        try {
          await this.agentService.rejectDecision(decision.id!);
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

  protected getStatusSeverity(
    status: string,
  ): "success" | "info" | "warn" | "danger" | "secondary" {
    switch (status) {
      case "completed":
      case "executed":
      case "approved":
        return "success";
      case "running":
      case "suggested":
        return "info";
      case "failed":
        return "danger";
      case "cancelled":
      case "rejected":
        return "warn";
      default:
        return "secondary";
    }
  }

  protected getDecisionTypeLabel(type: string): string {
    switch (type) {
      case "assign_load":
        return "Assign Load";
      case "create_trip":
        return "Create Trip";
      case "dispatch_trip":
        return "Dispatch Trip";
      case "book_load_board_load":
        return "Book Load";
      case "reassign_load":
        return "Reassign";
      default:
        return type;
    }
  }
}
