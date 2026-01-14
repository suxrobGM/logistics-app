import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import type { TableLazyLoadEvent } from "primeng/table";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import {
  Api,
  type DutyStatus,
  type HosLogDto,
  getDriverHosLogs,
  getEmployeeById,
} from "@/core/api";
import { LabeledField } from "@/shared/components";

@Component({
  selector: "app-eld-hos-logs",
  templateUrl: "./eld-hos-logs.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePicker,
    DatePipe,
    DecimalPipe,
    FormsModule,
    LabeledField,
    ProgressSpinnerModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class EldHosLogsComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);

  readonly employeeId = input.required<string>();

  protected readonly loading = signal(true);
  protected readonly error = signal<string | null>(null);
  protected readonly employeeName = signal<string>("Driver");
  protected readonly logs = signal<HosLogDto[]>([]);
  protected readonly totalRecords = signal(0);
  protected readonly pageSize = signal(25);
  protected readonly first = signal(0);

  // Date range filter - default to last 7 days
  protected startDate: Date = new Date(Date.now() - 7 * 24 * 60 * 60 * 1000);
  protected endDate: Date = new Date();
  protected readonly today: Date = new Date();

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

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    const employeeId = this.employeeId();

    this.loading.set(true);
    this.error.set(null);

    try {
      // Load employee info and logs in parallel
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

  protected formatMinutes(minutes: number): string {
    const hours = Math.floor(minutes / 60);
    const mins = minutes % 60;
    return `${hours}h ${mins}m`;
  }

  protected goBack(): void {
    this.router.navigate(["/eld"]);
  }
}
