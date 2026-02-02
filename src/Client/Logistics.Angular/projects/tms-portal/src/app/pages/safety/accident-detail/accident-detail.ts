import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import {
  Api,
  getAccidentReportById,
  submitAccidentReport,
  reviewAccidentReport,
  resolveAccidentReport,
} from "@logistics/shared/api";
import type {
  AccidentReportDto,
  AccidentReportStatus,
  AccidentSeverity,
} from "@logistics/shared/api";
import { AuthService } from "@/core/auth";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DialogModule } from "primeng/dialog";
import { TextareaModule } from "primeng/textarea";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { AccidentQuickInfo } from "../components";

@Component({
  selector: "app-accident-detail",
  templateUrl: "./accident-detail.html",
  imports: [
    RouterLink,
    FormsModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    CardModule,
    DialogModule,
    TextareaModule,
    ProgressSpinnerModule,
    TabsModule,
    TagModule,
    PageHeader,
    AccidentQuickInfo,
  ],
})
export class AccidentDetailPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly isSubmitting = signal(false);
  protected readonly report = signal<AccidentReportDto | null>(null);

  // Dialog visibility
  protected readonly showReviewDialog = signal(false);
  protected readonly showResolveDialog = signal(false);

  // Form data
  protected reviewNotes = "";
  protected resolutionNotes = "";

  protected readonly canSubmit = computed(() => this.report()?.status === "draft");
  protected readonly canEdit = computed(() => this.report()?.status === "draft");
  protected readonly canReview = computed(() => this.report()?.status === "submitted");
  protected readonly canResolve = computed(() => {
    const status = this.report()?.status;
    return status === "submitted" || status === "under_review";
  });

  protected readonly pageTitle = computed(() => {
    const rep = this.report();
    if (!rep) return "Accident Report";
    const date = rep.accidentDateTime
      ? new Date(rep.accidentDateTime).toLocaleDateString()
      : "Unknown Date";
    return `Accident - ${date}`;
  });

  async ngOnInit(): Promise<void> {
    await this.loadReport();
  }

  private async loadReport(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getAccidentReportById, { id: this.id() });
      if (result) {
        this.report.set(result);
      } else {
        this.toastService.showError("Accident report not found");
        this.router.navigateByUrl("/safety/accidents");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getStatusSeverity(status: AccidentReportStatus | undefined): TagSeverity {
    switch (status) {
      case "resolved":
        return "success";
      case "submitted":
        return "info";
      case "under_review":
        return "warn";
      case "insurance_filed":
        return "secondary";
      case "draft":
        return "contrast";
      default:
        return "secondary";
    }
  }

  protected getSeveritySeverity(severity: AccidentSeverity | undefined): TagSeverity {
    switch (severity) {
      case "minor":
        return "info";
      case "moderate":
        return "warn";
      case "severe":
        return "danger";
      case "fatal":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected editReport(): void {
    this.router.navigateByUrl(`/safety/accidents/${this.id()}/edit`);
  }

  protected submitReport(): void {
    this.toastService.confirm({
      message: "Are you sure you want to submit this accident report? Once submitted, it cannot be edited.",
      header: "Confirm Submission",
      icon: "pi pi-exclamation-triangle",
      accept: () => this.doSubmitReport(),
    });
  }

  private async doSubmitReport(): Promise<void> {
    this.isSubmitting.set(true);
    try {
      const result = await this.api.invoke(submitAccidentReport, { id: this.id() });
      if (result) {
        this.report.set(result);
        this.toastService.showSuccess("Accident report submitted successfully");
      }
    } catch {
      this.toastService.showError("Failed to submit accident report");
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected openReviewDialog(): void {
    this.reviewNotes = "";
    this.showReviewDialog.set(true);
  }

  protected async confirmReview(): Promise<void> {
    this.isSubmitting.set(true);
    try {
      const userId = this.authService.getUserData()?.id;
      if (!userId) {
        this.toastService.showError("User not authenticated");
        return;
      }

      const result = await this.api.invoke(reviewAccidentReport, {
        id: this.id(),
        body: {
          reviewedById: userId,
          reviewNotes: this.reviewNotes || null,
        },
      });

      if (result) {
        this.report.set(result);
        this.toastService.showSuccess("Accident report is now under review");
        this.showReviewDialog.set(false);
      }
    } catch {
      this.toastService.showError("Failed to start review");
    } finally {
      this.isSubmitting.set(false);
    }
  }

  protected openResolveDialog(): void {
    this.resolutionNotes = "";
    this.showResolveDialog.set(true);
  }

  protected async confirmResolve(): Promise<void> {
    this.isSubmitting.set(true);
    try {
      const result = await this.api.invoke(resolveAccidentReport, {
        id: this.id(),
        body: {
          resolutionNotes: this.resolutionNotes || null,
        },
      });

      if (result) {
        this.report.set(result);
        this.toastService.showSuccess("Accident report resolved successfully");
        this.showResolveDialog.set(false);
      }
    } catch {
      this.toastService.showError("Failed to resolve accident report");
    } finally {
      this.isSubmitting.set(false);
    }
  }
}
