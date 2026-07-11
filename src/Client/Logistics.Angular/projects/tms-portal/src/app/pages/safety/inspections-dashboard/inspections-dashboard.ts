import { DatePipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  getAccidentReports,
  getDvirReports,
  getInspections,
  getPendingDvirReviews,
  type AccidentReportDto,
  type AccidentReportStatus,
  type AccidentSeverity,
  type ConditionReportDto,
  type DvirReportDto,
  type DvirStatus,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Icon,
  Spinner,
  UiButton,
  UiDataTable,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { PageHeader } from "@/shared/components";

@Component({
  selector: "app-inspections-dashboard",
  templateUrl: "./inspections-dashboard.html",
  imports: [Badge, Card, DatePipe, Icon, PageHeader, Spinner, UiButton, UiDataTable],
})
export class InspectionsDashboardPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly isLoading = signal(true);

  // Stats
  protected readonly pendingDvirCount = signal(0);
  protected readonly openAccidentsCount = signal(0);
  protected readonly conditionReportsCount = signal(0);

  // Recent data
  protected readonly recentDvirs = signal<DvirReportDto[]>([]);
  protected readonly recentAccidents = signal<AccidentReportDto[]>([]);
  protected readonly recentConditionReports = signal<ConditionReportDto[]>([]);

  async ngOnInit(): Promise<void> {
    await this.loadDashboardData();
  }

  private async loadDashboardData(): Promise<void> {
    this.isLoading.set(true);
    try {
      const [pendingDvirs, dvirs, accidents, conditionReports] = await Promise.all([
        this.api.invoke(getPendingDvirReviews, {}),
        this.api.invoke(getDvirReports, { PageSize: 5, OrderBy: "-InspectionDate" }),
        this.api.invoke(getAccidentReports, { PageSize: 5, OrderBy: "-AccidentDateTime" }),
        this.api.invoke(getInspections, {}),
      ]);

      // Stats
      this.pendingDvirCount.set(pendingDvirs?.length ?? 0);
      this.openAccidentsCount.set(
        accidents?.items?.filter((a: AccidentReportDto) => a.status !== "resolved").length ?? 0,
      );
      this.conditionReportsCount.set(conditionReports?.length ?? 0);

      // Recent data
      this.recentDvirs.set(dvirs?.items ?? []);
      this.recentAccidents.set(accidents?.items ?? []);
      this.recentConditionReports.set(conditionReports?.slice(0, 5) ?? []);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getDvirStatusSeverity(status: DvirStatus | undefined): UiBadgeIntent {
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

  protected getAccidentStatusSeverity(status: AccidentReportStatus | undefined): UiBadgeIntent {
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

  protected getAccidentSeveritySeverity(severity: AccidentSeverity | undefined): UiBadgeIntent {
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

  protected viewConditionReport(report: ConditionReportDto): void {
    this.router.navigateByUrl(`/safety/condition-reports/${report.id}`);
  }

  protected navigateTo(route: string): void {
    this.router.navigateByUrl(route);
  }
}
