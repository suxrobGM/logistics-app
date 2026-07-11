import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import type { MaintenanceIntervalType, MaintenanceScheduleDto } from "@logistics/shared/api";
import {
  Badge,
  Card,
  Icon,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSortHeader,
  type UiBadgeIntent,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { DataContainer, PageHeader } from "@/shared/components";
import { UpcomingServiceStore } from "../store";

@Component({
  selector: "app-upcoming-service",
  templateUrl: "./upcoming-service.html",
  providers: [UpcomingServiceStore],
  imports: [
    Badge,
    Card,
    DataContainer,
    DatePipe,
    DecimalPipe,
    Icon,
    PageHeader,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSortHeader,
  ],
})
export class UpcomingServicePage implements OnInit {
  private readonly router = inject(Router);
  protected readonly store = inject(UpcomingServiceStore);

  protected readonly selectedRow = signal<MaintenanceScheduleDto | null>(null);

  protected readonly actionMenuItems: UiMenuItem[] = [
    {
      label: "Log service",
      icon: "check",
      command: () => this.logService(this.selectedRow()!),
    },
    {
      label: "View truck",
      icon: "truck",
      command: () => this.viewTruck(this.selectedRow()!),
    },
  ];

  ngOnInit(): void {
    this.store.load();
  }

  protected getStatusSeverity(schedule: MaintenanceScheduleDto): UiBadgeIntent {
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

  protected logService(schedule: MaintenanceScheduleDto): void {
    this.router.navigate(["/maintenance/records/add"], {
      queryParams: { truckId: schedule.truckId, type: schedule.type },
    });
  }

  protected viewTruck(schedule: MaintenanceScheduleDto): void {
    this.router.navigateByUrl(`/trucks/${schedule.truckId}`);
  }
}
