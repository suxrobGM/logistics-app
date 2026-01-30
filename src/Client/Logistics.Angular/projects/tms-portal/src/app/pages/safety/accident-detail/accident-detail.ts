import { Component, computed, inject, input, type OnInit, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { CurrencyPipe, DatePipe } from "@angular/common";
import { Api, getAccidentReportById } from "@logistics/shared/api";
import type { AccidentReportDto, AccidentReportStatus, AccidentSeverity } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { TagModule } from "primeng/tag";
import { PageHeader } from "@/shared/components";
import { ToastService } from "@/core/services";
import type { TagSeverity } from "@/shared/types";

@Component({
  selector: "app-accident-detail",
  templateUrl: "./accident-detail.html",
  imports: [
    RouterLink,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    TabsModule,
    TagModule,
    PageHeader,
  ],
})
export class AccidentDetailPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly report = signal<AccidentReportDto | null>(null);

  protected readonly pageTitle = computed(() => {
    const rep = this.report();
    if (!rep) return "Accident Report";
    const date = rep.accidentDateTime ? new Date(rep.accidentDateTime).toLocaleDateString() : "Unknown Date";
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
}
