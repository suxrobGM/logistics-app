import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { Router } from "@angular/router";
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
import { PanelModule } from "primeng/panel";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { Labels } from "@/shared/utils";

@Component({
  selector: "app-session-detail",
  templateUrl: "./session-detail.html",
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
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly session = signal<DispatchSessionDto | null>(null);
  protected readonly isLoading = signal(false);

  protected readonly Labels = Labels;

  ngOnInit(): void {
    this.loadSession();
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
