import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  type AccidentReportDto,
  type AccidentReportStatus,
  type AccidentSeverity,
  Api,
  submitAccidentReport,
} from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { AccidentsListStore } from "../store";

@Component({
  selector: "app-accidents-list",
  templateUrl: "./accidents-list.html",
  providers: [AccidentsListStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    TagModule,
    DatePipe,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class AccidentsListPage {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(AccidentsListStore);

  protected readonly selectedRow = signal<AccidentReportDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View details",
      icon: "pi pi-eye",
      command: () => this.viewDetails(this.selectedRow()!),
    },
    {
      label: "Submit report",
      icon: "pi pi-send",
      command: () => this.submitReport(this.selectedRow()!),
    },
  ];

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
