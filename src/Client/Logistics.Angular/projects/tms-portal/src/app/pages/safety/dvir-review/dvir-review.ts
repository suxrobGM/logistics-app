import { Component, inject, input, type OnInit, signal } from "@angular/core";
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { Api, getDvirReportById, reviewDvirReport } from "@logistics/shared/api";
import type { DvirReportDto, ReviewDvirReportCommand } from "@logistics/shared/api";
import { LabeledField, ValidationSummary } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TextareaModule } from "primeng/textarea";
import { ToggleSwitchModule } from "primeng/toggleswitch";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import { AuthService } from "@/core/auth";
import { DvirDefectsList } from "../components/dvir-defects-list/dvir-defects-list";

@Component({
  selector: "app-dvir-review",
  templateUrl: "./dvir-review.html",
  imports: [
    ReactiveFormsModule,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    TextareaModule,
    ToggleSwitchModule,
    PageHeader,
    LabeledField,
    ValidationSummary,
    DvirDefectsList,
  ],
})
export class DvirReviewPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly authService = inject(AuthService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly report = signal<DvirReportDto | null>(null);

  protected readonly form = new FormGroup({
    defectsCorrected: new FormControl<boolean>(false, { nonNullable: true }),
    mechanicNotes: new FormControl<string | null>(null),
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

  protected async submitReview(): Promise<void> {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.isSaving.set(true);
    try {
      const formValue = this.form.getRawValue();
      const userId = this.authService.getUserData()?.id;

      if (!userId) {
        this.toastService.showError("User not authenticated");
        return;
      }

      const command: ReviewDvirReportCommand = {
        reportId: this.id(),
        reviewedById: userId,
        defectsCorrected: formValue.defectsCorrected,
        mechanicNotes: formValue.mechanicNotes,
      };

      await this.api.invoke(reviewDvirReport, { id: this.id(), body: command });
      this.toastService.showSuccess("DVIR review submitted successfully");
      this.router.navigateByUrl(`/safety/dvir/${this.id()}`);
    } catch {
      this.toastService.showError("Failed to submit review");
    } finally {
      this.isSaving.set(false);
    }
  }

  protected cancel(): void {
    this.router.navigateByUrl(`/safety/dvir/${this.id()}`);
  }
}
