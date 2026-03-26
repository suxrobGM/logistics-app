import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router } from "@angular/router";
import { ConfirmationService } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { PanelModule } from "primeng/panel";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import type { DispatchDecisionDto, DispatchSessionDto } from "@logistics/shared/api";
import { ToastService } from "@/core/services";
import { DispatchAgentService } from "../store/dispatch-agent.service";

@Component({
  selector: "app-session-detail",
  templateUrl: "./session-detail.html",
  providers: [ConfirmationService],
  imports: [
    ButtonModule,
    TableModule,
    TagModule,
    TooltipModule,
    PanelModule,
    ConfirmDialogModule,
    DatePipe,
    DecimalPipe,
  ],
})
export class SessionDetailPage implements OnInit {
  private readonly agentService = inject(DispatchAgentService);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  private readonly confirmService = inject(ConfirmationService);

  readonly id = input.required<string>();

  protected readonly session = signal<DispatchSessionDto | null>(null);
  protected readonly isLoading = signal(false);

  ngOnInit(): void {
    this.loadSession();
  }

  protected async loadSession(): Promise<void> {
    this.isLoading.set(true);
    try {
      const session = await this.agentService.getSession(this.id());
      this.session.set(session);
    } catch {
      this.toastService.showError("Failed to load session");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async approveDecision(decision: DispatchDecisionDto): Promise<void> {
    try {
      await this.agentService.approveDecision(decision.id!);
      this.toastService.showSuccess("Decision approved and executed");
      await this.loadSession();
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
      await this.agentService.cancelSession(session.id!);
      this.toastService.showSuccess("Session cancelled");
      await this.loadSession();
    } catch {
      this.toastService.showError("Failed to cancel session");
    }
  }

  protected goBack(): void {
    this.router.navigate(["/ai-dispatch"]);
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
