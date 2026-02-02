import { CurrencyPipe, DatePipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { MaintenanceRecordDto, MaintenanceType } from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader, SearchInput } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { ServiceRecordsStore } from "../store";

@Component({
  selector: "app-service-records",
  templateUrl: "./service-records.html",
  providers: [ServiceRecordsStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    TagModule,
    DatePipe,
    CurrencyPipe,
    DecimalPipe,
    DataContainer,
    PageHeader,
    SearchInput,
  ],
})
export class ServiceRecordsPage {
  private readonly router = inject(Router);
  protected readonly store = inject(ServiceRecordsStore);

  protected readonly selectedRow = signal<MaintenanceRecordDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "View details",
      icon: "pi pi-eye",
      command: () => this.viewDetails(this.selectedRow()!),
    },
    {
      label: "Edit",
      icon: "pi pi-pencil",
      command: () => this.editRecord(this.selectedRow()!),
    },
  ];

  protected getTypeSeverity(type: MaintenanceType | undefined): TagSeverity {
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
