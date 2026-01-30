import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, computed, inject, input, type OnInit, signal } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { Api, getDvirReportById } from "@logistics/shared/api";
import type { DvirReportDto, DvirStatus, DvirType } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
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
    ButtonModule,
    CardModule,
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

  public readonly id = input.required<string>();

  protected readonly isLoading = signal(true);
  protected readonly report = signal<DvirReportDto | null>(null);

  protected readonly pageTitle = computed(() => {
    const r = this.report();
    if (!r) return "DVIR Report";
    return `DVIR #${r.truckNumber ?? "Unknown"} - ${this.getTypeLabel(r.type)}`;
  });

  protected readonly canReview = computed(() => {
    const r = this.report();
    return r?.status === "submitted" || r?.status === "requires_repair";
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
}
