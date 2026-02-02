import { CurrencyPipe, DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  type MaintenanceScheduleDto,
  type MaintenanceRecordDto,
  getUpcomingMaintenance,
  getMaintenanceRecords,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DashboardCard, PageHeader, StatCard } from "@/shared/components";

@Component({
  selector: "app-maintenance-dashboard",
  templateUrl: "./maintenance-dashboard.html",
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    TagModule,
    DatePipe,
    CurrencyPipe,
    DecimalPipe,
    PageHeader,
    StatCard,
    DashboardCard,
  ],
})
export class MaintenanceDashboardPage implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly upcomingMaintenance = signal<MaintenanceScheduleDto[]>([]);
  protected readonly recentRecords = signal<MaintenanceRecordDto[]>([]);

  protected readonly overdueCount = computed(() =>
    this.upcomingMaintenance().filter((m) => m.isOverdue).length,
  );

  protected readonly dueSoonCount = computed(() =>
    this.upcomingMaintenance().filter((m) => !m.isOverdue && (m.daysUntilDue ?? 0) <= 30).length,
  );

  protected readonly totalCostThisMonth = computed(() =>
    this.recentRecords()
      .filter((r) => {
        const serviceDate = new Date(r.serviceDate ?? "");
        const now = new Date();
        return (
          serviceDate.getMonth() === now.getMonth() &&
          serviceDate.getFullYear() === now.getFullYear()
        );
      })
      .reduce((sum, r) => sum + (r.totalCost ?? 0), 0),
  );

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const [upcomingData, recordsData] = await Promise.all([
        this.api.invoke(getUpcomingMaintenance, { daysAhead: 90, includeOverdue: true }),
        this.api.invoke(getMaintenanceRecords, { PageSize: 5, OrderBy: "-ServiceDate" }),
      ]);

      // getUpcomingMaintenance returns an array directly
      this.upcomingMaintenance.set(upcomingData ?? []);
      // getMaintenanceRecords returns a paged response
      this.recentRecords.set(recordsData?.items ?? []);
    } catch (err) {
      this.error.set("Failed to load maintenance data");
      console.error("Error loading maintenance data:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected viewAllUpcoming(): void {
    this.router.navigate(["/maintenance/upcoming"]);
  }

  protected viewAllRecords(): void {
    this.router.navigate(["/maintenance/records"]);
  }

  protected addServiceRecord(): void {
    this.router.navigate(["/maintenance/records/add"]);
  }

  protected viewTruckMaintenance(truckId: string): void {
    this.router.navigate(["/trucks", truckId, "maintenance"]);
  }

  protected viewServiceRecord(recordId: string): void {
    this.router.navigate(["/maintenance/records", recordId]);
  }
}
