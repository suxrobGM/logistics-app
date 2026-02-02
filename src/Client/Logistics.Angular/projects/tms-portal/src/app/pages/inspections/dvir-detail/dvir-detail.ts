import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, computed, inject, input, type OnInit, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  getDvirReportById,
  dismissDvirReport,
  rejectDvirReport,
} from "@logistics/shared/api";
import type { DvirReportDto, DvirStatus, DvirType } from "@logistics/shared/api";
import { AuthService } from "@/core/auth";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { TextareaModule } from "primeng/textarea";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TagModule } from "primeng/tag";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import type { TagSeverity } from "@/shared/types";
import { DvirDefectsList } from "../components/dvir-defects-list/dvir-defects-list";

@Component({
  selector: "app-dvir-detail",
  templateUrl: "./dvir-detail.html",
  imports: [
    DatePipe,
    DecimalPipe,
    FormsModule,
    ButtonModule,
    CardModule,
    DialogModule,
    TextareaModule,
    ProgressSpinnerModule,
    TagModule,
    RouterLink,
    PageHeader,
    DvirDefectsList,
  ],
})
export class DvirDetailPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly isSubmitting = signal(false);
  protected readonly report = signal<DvirReportDto | null>(null);

  // Dialog visibility
  protected readonly showDismissDialog = signal(false);
  protected readonly showRejectDialog = signal(false);

  // Form data
  protected dismissNotes = "";
  protected rejectionReason = "";

  protected readonly pageTitle = computed(() => {
    const r = this.report();
    if (!r) return "DVIR Report";
    return `DVIR #${r.truckNumber ?? "Unknown"} - ${this.getTypeLabel(r.type)}`;
  });

  protected readonly canReview = computed(() => {
    const r = this.report();
    return r?.status === "submitted" || r?.status === "requires_repair";
  });

  protected readonly canDismiss = computed(() => {
    const r = this.report();
    // Can dismiss submitted DVIRs with no defects (quick clear)
    return r?.status === "submitted" && (!r.defects || r.defects.length === 0);
  });

  protected readonly canReject = computed(() => {
    const r = this.report();
    // Can reject submitted DVIRs (send back to driver)
    return r?.status === "submitted";
  });

  async ngOnInit(): Promise<void> {
    await this.loadReport();
  }

  private async loadReport(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getDvirReportById, { id: this.id() });
      if (result) {
        this.report.set(result);
      } else {
        this.toastService.showError("DVIR report not found");
        this.router.navigateByUrl("/safety/dvir");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getStatusSeverity(status: DvirStatus | undefined): TagSeverity {
    switch (status) {
      case "cleared":
        return "success";
      case "submitted":
        return "info";
      case "requires_repair":
        return "danger";
      case "reviewed":
        return "secondary";
      case "draft":
        return "warn";
      case "rejected":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected getStatusLabel(status: DvirStatus | undefined): string {
    switch (status) {
      case "cleared":
        return "Cleared";
      case "submitted":
        return "Submitted";
      case "requires_repair":
        return "Requires Repair";
      case "reviewed":
        return "Reviewed";
      case "draft":
        return "Draft";
      case "rejected":
        return "Rejected";
      default:
        return status ?? "Unknown";
    }
  }

  protected getTypeSeverity(type: DvirType | undefined): TagSeverity {
    return type === "pre_trip" ? "info" : "secondary";
  }

  protected getTypeLabel(type: DvirType | undefined): string {
    return type === "pre_trip" ? "Pre-Trip" : "Post-Trip";
  }

  protected reviewReport(): void {
    this.router.navigateByUrl(`/safety/dvir/${this.id()}/review`);
  }

  protected openDismissDialog(): void {
    this.dismissNotes = "";
    this.showDismissDialog.set(true);
  }

  protected async confirmDismiss(): Promise<void> {
    this.isSubmitting.set(true);
    try {
      const userId = this.authService.getUserData()?.id;
      if (!userId) {
        this.toastService.showError("User not authenticated");
        return;
      }

      const result = await this.api.invoke(dismissDvirReport, {
        id: this.id(),
        body: {
          dismissedById: userId,
          notes: this.dismissNotes || null,
        },
      });

      if (result) {
        this.report.set(result);
        this.toastService.showSuccess("DVIR report dismissed successfully");
        this.showDismissDialog.set(false);
      }
    } catch {
      this.toastService.showError("Failed to dismiss DVIR report");
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected openRejectDialog(): void {
    this.rejectionReason = "";
    this.showRejectDialog.set(true);
  }

  protected async confirmReject(): Promise<void> {
    if (!this.rejectionReason.trim()) {
      this.toastService.showError("Please provide a rejection reason");
      return;
    }

    this.isSubmitting.set(true);
    try {
      const userId = this.authService.getUserData()?.id;
      if (!userId) {
        this.toastService.showError("User not authenticated");
        return;
      }

      const result = await this.api.invoke(rejectDvirReport, {
        id: this.id(),
        body: {
          rejectedById: userId,
          rejectionReason: this.rejectionReason,
        },
      });

      if (result) {
        this.report.set(result);
        this.toastService.showSuccess("DVIR report rejected and sent back to driver");
        this.showRejectDialog.set(false);
      }
    } catch {
      this.toastService.showError("Failed to reject DVIR report");
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
