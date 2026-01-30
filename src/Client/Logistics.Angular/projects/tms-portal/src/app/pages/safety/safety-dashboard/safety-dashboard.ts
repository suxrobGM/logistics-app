import { Component, inject, type OnInit, signal } from "@angular/core";
import { Router } from "@angular/router";
import { DatePipe } from "@angular/common";
import {
  Api,
  getPendingDvirReviews,
  getDvirReports,
  getAccidentReports,
  getActiveEmergencyAlerts,
} from "@logistics/shared/api";
import type {
  DvirReportDto,
  AccidentReportDto,
  EmergencyAlertDto,
  AccidentReportStatus,
  AccidentSeverity,
  DvirStatus,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { PageHeader } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";

@Component({
  selector: "app-safety-dashboard",
  templateUrl: "./safety-dashboard.html",
  imports: [
    DatePipe,
    ButtonModule,
    CardModule,
    ProgressSpinnerModule,
    TableModule,
    TagModule,
    PageHeader,
  ],
})
export class SafetyDashboardPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly isLoading = signal(true);

  // Stats
  protected readonly pendingDvirCount = signal(0);
  protected readonly openAccidentsCount = signal(0);
  protected readonly activeAlertsCount = signal(0);

  // Recent data
  protected readonly recentDvirs = signal<DvirReportDto[]>([]);
  protected readonly recentAccidents = signal<AccidentReportDto[]>([]);
  protected readonly activeAlerts = signal<EmergencyAlertDto[]>([]);

  async ngOnInit(): Promise<void> {
    await this.loadDashboardData();
  }

  private async loadDashboardData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [pendingDvirs, dvirs, accidents, alerts] = await Promise.all([
        this.api.invoke(getPendingDvirReviews, {}),
        this.api.invoke(getDvirReports, { PageSize: 5, OrderBy: "InspectionDate desc" }),
        this.api.invoke(getAccidentReports, { PageSize: 5, OrderBy: "AccidentDateTime desc" }),
        this.api.invoke(getActiveEmergencyAlerts, {}),
      ]);

      // Stats
      this.pendingDvirCount.set(pendingDvirs?.length ?? 0);
      this.openAccidentsCount.set(
        accidents?.items?.filter((a: AccidentReportDto) => a.status !== "resolved").length ?? 0
      );
      this.activeAlertsCount.set(alerts?.items?.length ?? 0);

      // Recent data
      this.recentDvirs.set(dvirs?.items ?? []);
      this.recentAccidents.set(accidents?.items ?? []);
      this.activeAlerts.set(alerts?.items ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getDvirStatusSeverity(status: DvirStatus | undefined): TagSeverity {
    switch (status) {
      case "cleared":
        return "success";
      case "reviewed":
        return "info";
      case "submitted":
        return "warn";
      case "requires_repair":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected getAccidentStatusSeverity(status: AccidentReportStatus | undefined): TagSeverity {
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

  protected getAccidentSeveritySeverity(severity: AccidentSeverity | undefined): TagSeverity {
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

  protected viewDvir(dvir: DvirReportDto): void {
    this.router.navigateByUrl(`/safety/dvir/${dvir.id}`);
  }

  protected viewAccident(accident: AccidentReportDto): void {
    this.router.navigateByUrl(`/safety/accidents/${accident.id}`);
  }

  protected navigateTo(route: string): void {
    this.router.navigateByUrl(route);
  }
}
