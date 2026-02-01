import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { DvirReportDto, DvirStatus, DvirType } from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { DvirListStore } from "../store";

@Component({
  selector: "app-dvir-list",
  templateUrl: "./dvir-list.html",
  providers: [DvirListStore],
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
export class DvirListPage {
  private readonly router = inject(Router);
  protected readonly store = inject(DvirListStore);

  protected readonly selectedRow = signal<DvirReportDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View details",
      icon: "pi pi-eye",
      command: () => this.viewDetails(this.selectedRow()!),
    },
    {
      label: "Review",
      icon: "pi pi-check-circle",
      command: () => this.reviewDvir(this.selectedRow()!),
    },
  ];

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
    switch (type) {
      case "pre_trip":
        return "info";
      case "post_trip":
        return "secondary";
      default:
        return "secondary";
    }
  }

  protected getTypeLabel(type: DvirType | undefined): string {
    switch (type) {
      case "pre_trip":
        return "Pre-Trip";
      case "post_trip":
        return "Post-Trip";
      default:
        return type ?? "Unknown";
    }
  }

  protected onRowClick(dvir: DvirReportDto): void {
    this.router.navigateByUrl(`/safety/dvir/${dvir.id}`);
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected viewDetails(dvir: DvirReportDto): void {
    this.router.navigateByUrl(`/safety/dvir/${dvir.id}`);
  }

  protected reviewDvir(dvir: DvirReportDto): void {
    this.router.navigateByUrl(`/safety/dvir/${dvir.id}/review`);
  }
}
