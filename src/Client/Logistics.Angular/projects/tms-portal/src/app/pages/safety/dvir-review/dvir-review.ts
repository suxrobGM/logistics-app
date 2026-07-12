import { Component, inject, input, signal, type OnInit } from "@angular/core";
import { form, FormField, FormRoot } from "@angular/forms/signals";
import { Router } from "@angular/router";
import {
  Api,
  getDvirReportById,
  reviewDvirReport,
  type DvirReportDto,
  type ReviewDvirReportCommand,
} from "@logistics/shared/api";
import {
  Card,
  Spinner,
  UiButton,
  UiFormField,
  UiTextareaField,
  UiToggleField,
  ValidatedForm,
} from "@logistics/shared/ui";
import { AuthService } from "@/core/auth";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { DvirDefectsList } from "../_components/dvir-defects-list/dvir-defects-list";

@Component({
  selector: "app-dvir-review",
  templateUrl: "./dvir-review.html",
  imports: [
    Card,
    DvirDefectsList,
    FormField,
    FormRoot,
    PageHeader,
    Spinner,
    UiButton,
    UiFormField,
    UiTextareaField,
    UiToggleField,
    ValidatedForm,
  ],
})
export class DvirReviewPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly report = signal<DvirReportDto | null>(null);

  protected readonly model = signal<{ defectsCorrected: boolean; mechanicNotes: string }>({
    defectsCorrected: false,
    mechanicNotes: "",
  });

  /** The review fields are all optional, so the form declares no validation rules. */
  protected readonly form = form(
    this.model,
    () => {
      // No validation rules — DVIR review fields are all optional.
    },
    {
      submission: {
        action: async () => {
          const userId = this.authService.getUserData()?.id;
          if (!userId) {
            this.toastService.showError("User not authenticated");
            return undefined;
          }

          const v = this.model();
          const command: ReviewDvirReportCommand = {
            reportId: this.id(),
            reviewedById: userId,
            defectsCorrected: v.defectsCorrected,
            mechanicNotes: v.mechanicNotes || null,
          };

          try {
            await this.api.invoke(reviewDvirReport, { id: this.id(), body: command });
          } catch {
            this.toastService.showError("Failed to submit review");
            return undefined;
          }

          this.toastService.showSuccess("DVIR review submitted successfully");
          this.router.navigateByUrl(`/safety/dvir/${this.id()}`);
          return undefined;
        },
      },
    },
  );

  async ngOnInit(): Promise<void> {
    await this.loadReport();
  }

  private async loadReport(): Promise<void> {
    this.isLoading.set(true);
    try {
      const result = await this.api.invoke(getDvirReportById, { id: this.id() });
      if (result) {
        this.report.set(result);

        // Check if already reviewed
        if (result.status === "cleared" || result.status === "reviewed") {
          this.toastService.showWarning("This DVIR has already been reviewed");
          this.router.navigateByUrl(`/safety/dvir/${this.id()}`);
          return;
        }
      } else {
        this.toastService.showError("DVIR report not found");
        this.router.navigateByUrl("/safety/dvir");
      }
    } finally {
      this.isLoading.set(false);
    }
  }

  protected cancel(): void {
    this.router.navigateByUrl(`/safety/dvir/${this.id()}`);
  }
}
