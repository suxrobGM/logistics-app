import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { MaintenanceRecordDto, MaintenanceType } from "@logistics/shared/api";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import {
  Badge,
  Card,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { DataContainer, PageHeader, SearchField } from "@/shared/components";
import { ServiceRecordsStore } from "../store";

@Component({
  selector: "app-service-records",
  templateUrl: "./service-records.html",
  providers: [ServiceRecordsStore],
  imports: [
    Badge,
    Card,
    CurrencyFormatPipe,
    DataContainer,
    DatePipe,
    DecimalPipe,
    PageHeader,
    SearchField,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
  ],
})
export class ServiceRecordsPage {
  private readonly router = inject(Router);
  protected readonly store = inject(ServiceRecordsStore);

  protected readonly selectedRow = signal<MaintenanceRecordDto | null>(null);

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "View details",
      icon: "eye",
      command: () => this.viewDetails(this.selectedRow()!),
    },
    {
      label: "Edit",
      icon: "pencil",
      command: () => this.editRecord(this.selectedRow()!),
    },
  ];

  protected getTypeSeverity(type: MaintenanceType | undefined): UiBadgeIntent {
    switch (type) {
      case "oil_change":
        return "info";
      case "brake_inspection":
        return "warn";
      case "tire_rotation":
        return "secondary";
      case "annual_dot_inspection":
        return "danger";
      default:
        return "secondary";
    }
  }

  protected onSearch(value: string): void {
    this.store.setSearch(value);
  }

  protected addRecord(): void {
    this.router.navigate(["/maintenance/records/add"]);
  }

  protected viewDetails(record: MaintenanceRecordDto): void {
    this.router.navigateByUrl(`/maintenance/records/${record.id}`);
  }

  protected editRecord(record: MaintenanceRecordDto): void {
    this.router.navigateByUrl(`/maintenance/records/${record.id}/edit`);
  }
}
