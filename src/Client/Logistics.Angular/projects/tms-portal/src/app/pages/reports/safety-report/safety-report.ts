import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, computed, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { getSafetyReport } from "@logistics/shared/api";
import type {
  SafetyReportDto,
  SafetyTrendDto,
  SafetyStatusBreakdownDto,
  SafetySeverityBreakdownDto,
  SafetyEventTypeBreakdownDto,
} from "@logistics/shared/api";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import {
  BaseReportComponent,
  DashboardCard,
  DateRangePicker,
  PageHeader,
  type ReportQueryParams,
  StatCard,
} from "@/shared/components";
import { Converters } from "@/shared/utils";

// Status colors for DVIR
const DVIR_STATUS_COLORS = {
  background: ["#64748b", "#f59e0b", "#22c55e", "#ef4444", "#3b82f6"],
  hover: ["#475569", "#d97706", "#16a34a", "#dc2626", "#2563eb"],
};

// Severity colors for accidents
const SEVERITY_COLORS = {
  background: ["#22c55e", "#f59e0b", "#ef4444"],
  hover: ["#16a34a", "#d97706", "#dc2626"],
};

// Event type colors
const EVENT_TYPE_COLORS = [
  "#3b82f6",
  "#ef4444",
  "#f59e0b",
  "#22c55e",
  "#8b5cf6",
  "#ec4899",
  "#06b6d4",
  "#84cc16",
];

@Component({
  selector: "app-safety-report",
  templateUrl: "./safety-report.html",
  imports: [
    TableModule,
    ChartModule,
    DateRangePicker,
    CurrencyPipe,
    DecimalPipe,
    SkeletonModule,
    TagModule,
    PageHeader,
    StatCard,
    DashboardCard,
    RouterModule,
  ],
})
export class SafetyReportComponent extends BaseReportComponent<SafetyReportDto> implements OnInit {
  protected readonly dvirStatusChartData = signal<Record<string, unknown>>({});
  protected readonly accidentSeverityChartData = signal<Record<string, unknown>>({});
  protected readonly behaviorEventChartData = signal<Record<string, unknown>>({});
  protected readonly trendChartData = signal<Record<string, unknown>>({});

  protected readonly hasDvirStatusData = computed(() => Object.keys(this.dvirStatusChartData()).length > 0);
  protected readonly hasAccidentSeverityData = computed(
    () => Object.keys(this.accidentSeverityChartData()).length > 0,
  );
  protected readonly hasBehaviorEventData = computed(() => Object.keys(this.behaviorEventChartData()).length > 0);
  protected readonly hasTrendData = computed(() => Object.keys(this.trendChartData()).length > 0);

  protected readonly pieChartOptions = {
    plugins: {
      legend: { position: "bottom" as const },
    },
    maintainAspectRatio: false,
  };

  protected readonly barChartOptions = {
    plugins: {
      legend: { display: false },
    },
    scales: {
      y: { beginAtZero: true },
    },
    maintainAspectRatio: false,
  };

  protected readonly trendChartOptions = {
    plugins: {
      legend: { position: "bottom" as const },
    },
    scales: {
      y: { beginAtZero: true },
    },
    maintainAspectRatio: false,
  };

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override async query(params: ReportQueryParams): Promise<SafetyReportDto> {
    return this.api.invoke(getSafetyReport, {
      StartDate: params.startDate.toISOString(),
      EndDate: params.endDate?.toISOString(),
    });
  }

  protected override drawChart(result: SafetyReportDto): void {
    // DVIR Status breakdown pie chart
    const dvirStatus = result.dvirStatusBreakdown ?? [];
    if (dvirStatus.length > 0) {
      this.dvirStatusChartData.set({
        labels: dvirStatus.map((s: SafetyStatusBreakdownDto) => s.statusDisplay || s.status),
        datasets: [
          {
            data: dvirStatus.map((s: SafetyStatusBreakdownDto) => s.count),
            backgroundColor: DVIR_STATUS_COLORS.background.slice(0, dvirStatus.length),
            hoverBackgroundColor: DVIR_STATUS_COLORS.hover.slice(0, dvirStatus.length),
          },
        ],
      });
    }

    // Accident severity breakdown pie chart
    const accidentSeverity = result.accidentSeverityBreakdown ?? [];
    if (accidentSeverity.length > 0) {
      this.accidentSeverityChartData.set({
        labels: accidentSeverity.map((s: SafetySeverityBreakdownDto) => s.severityDisplay || s.severity),
        datasets: [
          {
            data: accidentSeverity.map((s: SafetySeverityBreakdownDto) => s.count),
            backgroundColor: SEVERITY_COLORS.background.slice(0, accidentSeverity.length),
            hoverBackgroundColor: SEVERITY_COLORS.hover.slice(0, accidentSeverity.length),
          },
        ],
      });
    }

    // Behavior event breakdown bar chart
    const behaviorEvents = result.behaviorEventBreakdown ?? [];
    if (behaviorEvents.length > 0) {
      this.behaviorEventChartData.set({
        labels: behaviorEvents.map((e: SafetyEventTypeBreakdownDto) => e.eventTypeDisplay || e.eventType),
        datasets: [
          {
            label: "Events",
            data: behaviorEvents.map((e: SafetyEventTypeBreakdownDto) => e.count),
            backgroundColor: EVENT_TYPE_COLORS.slice(0, behaviorEvents.length),
          },
        ],
      });
    }

    // Combined trends line chart
    const dvirTrends = result.dvirTrends ?? [];
    const accidentTrends = result.accidentTrends ?? [];
    const behaviorTrends = result.behaviorTrends ?? [];

    if (dvirTrends.length > 0 || accidentTrends.length > 0 || behaviorTrends.length > 0) {
      const periods = dvirTrends.map((t: SafetyTrendDto) => t.period);
      this.trendChartData.set({
        labels: periods,
        datasets: [
          {
            label: "DVIR Reports",
            data: dvirTrends.map((t: SafetyTrendDto) => t.count),
            borderColor: "#3b82f6",
            backgroundColor: "rgba(59, 130, 246, 0.1)",
            tension: 0.4,
            fill: true,
          },
          {
            label: "Accidents",
            data: accidentTrends.map((t: SafetyTrendDto) => t.count),
            borderColor: "#ef4444",
            backgroundColor: "rgba(239, 68, 68, 0.1)",
            tension: 0.4,
            fill: true,
          },
          {
            label: "Behavior Events",
            data: behaviorTrends.map((t: SafetyTrendDto) => t.count),
            borderColor: "#f59e0b",
            backgroundColor: "rgba(245, 158, 11, 0.1)",
            tension: 0.4,
            fill: true,
          },
        ],
      });
    }
  }

  protected getDefectRateClass(rate?: number): string {
    const value = rate ?? 0;
    if (value >= 20) return "text-red-600";
    if (value >= 10) return "text-amber-600";
    return "text-green-600";
  }

  protected getDriverInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
