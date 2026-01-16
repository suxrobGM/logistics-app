import { DatePipe } from "@angular/common";
import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  type DriverHosStatusDto,
  getAllDriversHos,
  syncAllDriversHos,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DashboardCard, PageHeader, StatCard } from "@/shared/components";

@Component({
  selector: "app-eld-dashboard",
  templateUrl: "./eld-dashboard.html",
  imports: [
    ButtonModule,
    TooltipModule,
    CardModule,
    TableModule,
    TagModule,
    DatePipe,
    ProgressSpinnerModule,
    PageHeader,
    StatCard,
    DashboardCard,
  ],
})
export class EldDashboardComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly drivers = signal<DriverHosStatusDto[]>([]);

  protected readonly availableDrivers = computed(() =>
    this.drivers().filter((d) => d.isAvailableForDispatch),
  );

  protected readonly violationCount = computed(
    () => this.drivers().filter((d) => d.isInViolation).length,
  );

  ngOnInit(): void {
    this.loadDriversHos();
  }

  protected async loadDriversHos(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const data = await this.api.invoke(getAllDriversHos);
      this.drivers.set(data ?? []);
    } catch (err) {
      this.error.set("Failed to load driver HOS status");
      console.error("Error loading HOS data:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async syncDriversHos(): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(syncAllDriversHos);
      await this.loadDriversHos();
    } catch (err) {
      this.error.set("Failed to sync driver HOS status");
      console.error("Error syncing HOS data:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected getDutyStatusSeverity(
    status: number,
  ): "success" | "info" | "warn" | "danger" | "secondary" {
    switch (status) {
      case 0: // OffDuty
        return "secondary";
      case 1: // SleeperBerth
        return "info";
      case 2: // Driving
        return "success";
      case 3: // OnDutyNotDriving
        return "warn";
      default:
        return "secondary";
    }
  }

  protected viewDriverLogs(employeeId: string): void {
    this.router.navigate(["/eld/drivers", employeeId, "logs"]);
  }

  protected configureProviders(): void {
    this.router.navigate(["/eld/providers"]);
  }
}
