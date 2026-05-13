import { DatePipe } from "@angular/common";
import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  getAllDriversHos,
  syncAllDriversHos,
  type DriverHosStatusDto,
} from "@logistics/shared/api";
import { EmptyState, ErrorState, Grid, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { EldRulesService } from "@/core/services";
import { DashboardCard, PageHeader, StatCard } from "@/shared/components";

const DEFAULT_DRIVING_WARN_MINUTES = 60;
const DEFAULT_ON_DUTY_WARN_MINUTES = 120;
const DRIVING_WARN_PCT = 0.1;
const ON_DUTY_WARN_PCT = 0.15;

@Component({
  selector: "app-eld-dashboard",
  templateUrl: "./eld-dashboard.html",
  imports: [
    ButtonModule,
    DashboardCard,
    DatePipe,
    EmptyState,
    ErrorState,
    Grid,
    PageHeader,
    ProgressSpinnerModule,
    Stack,
    StatCard,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class EldDashboardComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  protected readonly rules = inject(EldRulesService);

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly drivers = signal<DriverHosStatusDto[]>([]);

  protected readonly availableDrivers = computed(() =>
    this.drivers().filter((d) => d.isAvailableForDispatch),
  );

  protected readonly violationCount = computed(
    () => this.drivers().filter((d) => d.isInViolation).length,
  );

  protected readonly drivingWarnMinutes = computed(() => {
    const max = this.rules.limits()?.maxDailyDrivingMinutes;
    return max ? Math.round(max * DRIVING_WARN_PCT) : DEFAULT_DRIVING_WARN_MINUTES;
  });

  protected readonly onDutyWarnMinutes = computed(() => {
    const max = this.rules.limits()?.maxDailyOnDutyMinutes;
    return max ? Math.round(max * ON_DUTY_WARN_PCT) : DEFAULT_ON_DUTY_WARN_MINUTES;
  });

  ngOnInit(): void {
    this.rules.load();
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
