import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  submitAccidentReport,
  type AccidentReportDto,
  type AccidentReportStatus,
  type AccidentSeverity,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Icon,
  Stack,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchField } from "@/shared/components";
import { AccidentsListStore } from "../store";

@Component({
  selector: "app-accidents-list",
  templateUrl: "./accidents-list.html",
  providers: [AccidentsListStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    DatePipe,
    Icon,
    PageHeader,
    SearchField,
    Stack,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
  ],
})
export class AccidentsListPage {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(AccidentsListStore);

  protected readonly selectedRow = signal<AccidentReportDto | null>(null);

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "View details",
      icon: "eye",
      command: () => this.viewDetails(this.selectedRow()!),
    },
    {
      label: "Submit report",
      icon: "send",
      command: () => this.submitReport(this.selectedRow()!),
    },
  ];

  protected getStatusSeverity(status: AccidentReportStatus | undefined): UiBadgeIntent {
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

  protected getStatusLabel(status: AccidentReportStatus | undefined): string {
    switch (status) {
      case "resolved":
        return "Resolved";
      case "submitted":
        return "Submitted";
      case "under_review":
        return "Under Review";
      case "insurance_filed":
        return "Insurance Filed";
      case "draft":
        return "Draft";
      default:
        return status ?? "Unknown";
    }
  }

  protected getSeveritySeverity(severity: AccidentSeverity | undefined): UiBadgeIntent {
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

  protected getSeverityLabel(severity: AccidentSeverity | undefined): string {
    switch (severity) {
      case "minor":
        return "Minor";
      case "moderate":
        return "Moderate";
      case "severe":
        return "Severe";
      case "fatal":
        return "Fatal";
      default:
        return severity ?? "Unknown";
    }
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addAccident(): void {
    this.router.navigate(["/safety/accidents/add"]);
  }

  protected viewDetails(accident: AccidentReportDto): void {
    this.router.navigateByUrl(`/safety/accidents/${accident.id}`);
  }

  protected async submitReport(accident: AccidentReportDto): Promise<void> {
    if (accident.status !== "draft") {
      this.toastService.showError("Only draft reports can be submitted");
      return;
    }

    try {
      await this.api.invoke(submitAccidentReport, { id: accident.id! });
      this.toastService.showSuccess("Accident report submitted successfully");
      this.store.load();
    } catch {
      this.toastService.showError("Failed to submit accident report");
    }
  }
}
