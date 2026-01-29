import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import type { MaintenanceIntervalType, MaintenanceScheduleDto } from "@logistics/shared/api";
import type { MenuItem } from "primeng/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MenuModule } from "primeng/menu";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, PageHeader } from "@/shared/components";
import type { TagSeverity } from "@/shared/types";
import { UpcomingServiceStore } from "../store";

@Component({
  selector: "app-upcoming-service",
  templateUrl: "./upcoming-service.html",
  providers: [UpcomingServiceStore],
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    MenuModule,
    TagModule,
    DatePipe,
    DecimalPipe,
    DataContainer,
    PageHeader,
  ],
})
export class UpcomingServicePage implements OnInit {
  private readonly router = inject(Router);
  protected readonly store = inject(UpcomingServiceStore);

  protected readonly selectedRow = signal<MaintenanceScheduleDto | null>(null);

  protected readonly actionMenuItems: MenuItem[] = [
    {
      label: "Log service",
      icon: "pi pi-check",
      command: () => this.logService(this.selectedRow()!),
    },
    {
      label: "View truck",
      icon: "pi pi-truck",
      command: () => this.viewTruck(this.selectedRow()!),
    },
  ];

  ngOnInit(): void {
    this.store.load();
  }

  protected getStatusSeverity(schedule: MaintenanceScheduleDto): TagSeverity {
    if (schedule.isOverdue) {
      return "danger";
    }
    if ((schedule.daysUntilDue ?? 0) <= 7) {
      return "warn";
    }
    return "info";
  }

  protected getStatusLabel(schedule: MaintenanceScheduleDto): string {
    if (schedule.isOverdue) {
      return "Overdue";
    }
    if ((schedule.daysUntilDue ?? 0) <= 7) {
      return "Due Soon";
    }
    return "Scheduled";
  }

  protected getIntervalTypeLabel(type: MaintenanceIntervalType | undefined): string {
    switch (type) {
      case "mileage":
        return "Mileage";
      case "time_based":
        return "Time-Based";
      case "engine_hours":
        return "Engine Hours";
      case "combined":
        return "Combined";
      default:
        return type ?? "Unknown";
    }
  }

  protected onRowClick(schedule: MaintenanceScheduleDto): void {
    this.router.navigateByUrl(`/trucks/${schedule.truckId}/maintenance`);
  }

  protected logService(schedule: MaintenanceScheduleDto): void {
    this.router.navigate(["/maintenance/records/add"], {
      queryParams: { truckId: schedule.truckId, type: schedule.type },
    });
  }

  protected viewTruck(schedule: MaintenanceScheduleDto): void {
    this.router.navigateByUrl(`/trucks/${schedule.truckId}`);
  }
}
