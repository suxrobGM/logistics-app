import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  Api,
  getDriverHosLogs,
  getEmployeeById,
  type DutyStatus,
  type HosLogDto,
} from "@logistics/shared/api";
import { EmptyState, ErrorState, Grid, Stack } from "@logistics/shared/components";
import { LocalizationService } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePicker } from "primeng/datepicker";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule, type TableLazyLoadEvent } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { DashboardCard, FormField, PageHeader, StatCard } from "@/shared/components";

@Component({
  selector: "app-eld-hos-logs",
  templateUrl: "./eld-hos-logs.html",
  imports: [
    ButtonModule,
    CardModule,
    DashboardCard,
    DatePicker,
    DatePipe,
    DecimalPipe,
    EmptyState,
    ErrorState,
    FormsModule,
    FormField,
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
export class EldHosLogsComponent implements OnInit {
  private readonly api = inject(Api);
  protected readonly localization = inject(LocalizationService);

  public readonly employeeId = input.required<string>();

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly employeeName = signal<string>("Driver");
  protected readonly logs = signal<HosLogDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly first = signal(0);

  protected startDate = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);
  protected endDate = new Date();
  protected readonly today = new Date();

  protected readonly headerTitle = computed(() => `HOS Logs - ${this.employeeName()}`);

  protected readonly totalDrivingMinutes = computed(() =>
    this.logs()
      .filter((l) => l.dutyStatus === "driving")
      .reduce((sum, l) => sum + (l.durationMinutes ?? 0), 0),
  );

  protected readonly totalOnDutyMinutes = computed(() =>
    this.logs()
      .filter((l) => l.dutyStatus === "driving" || l.dutyStatus === "on_duty_not_driving")
      .reduce((sum, l) => sum + (l.durationMinutes ?? 0), 0),
  );

  protected readonly totalDrivingDisplay = computed(() =>
    this.localization.formatHosDuration(this.totalDrivingMinutes()),
  );

  protected readonly totalOnDutyDisplay = computed(() =>
    this.localization.formatHosDuration(this.totalOnDutyMinutes()),
  );

  protected readonly dateFormat = computed(() => this.localization.getPrimeNgDateFormat());

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    const employeeId = this.employeeId();

    this.loading.set(true);
    this.error.set(null);

    try {
      const [employee, logsResponse] = await Promise.all([
        this.api.invoke(getEmployeeById, { userId: employeeId }),
        this.api.invoke(getDriverHosLogs, {
          employeeId,
          StartDate: this.startDate.toISOString(),
          EndDate: this.endDate.toISOString(),
          Page: Math.floor(this.first() / this.pageSize()) + 1,
          PageSize: this.pageSize(),
          OrderBy: "startTime desc",
        }),
      ]);

      if (employee) {
        this.employeeName.set(employee.fullName ?? "Driver");
      }

      this.logs.set(logsResponse?.items ?? []);
      this.totalRecords.set(logsResponse?.pagination?.total ?? 0);
    } catch (err) {
      this.error.set("Failed to load HOS logs");
      console.error("Error loading HOS logs:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onLazyLoad(event: TableLazyLoadEvent): Promise<void> {
    this.first.set(event.first ?? 0);
    this.pageSize.set(event.rows ?? 25);
    await this.loadData();
  }

  protected async onDateRangeChange(): Promise<void> {
    this.first.set(0);
    await this.loadData();
  }

  protected getDutyStatusSeverity(
    status?: DutyStatus,
  ): "success" | "info" | "warn" | "danger" | "secondary" {
    switch (status) {
      case "off_duty":
        return "secondary";
      case "sleeper_berth":
        return "info";
      case "driving":
        return "success";
      case "on_duty_not_driving":
        return "warn";
      default:
        return "secondary";
    }
  }
}
