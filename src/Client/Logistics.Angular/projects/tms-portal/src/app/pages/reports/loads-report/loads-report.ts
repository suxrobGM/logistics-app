import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, signal } from "@angular/core";
import { getLoadsReport } from "@logistics/shared/api";
import type {
  LoadPerformanceDto,
  LoadTrendDto,
  LoadsReportDto,
  StatusDto,
  TypeDto,
} from "@logistics/shared/api";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import {
  BaseReportComponent,
  DashboardCard,
  DateRangePicker,
  PageHeader,
  type ReportQueryParams,
  StatCard,
} from "@/shared/components";
import {
  LOADS_CHART_PALETTE,
  getLoadsPerformanceChartOptions,
  getLoadsPieOptions,
  getLoadsTrendChartOptions,
  getLoadsTypeChartOptions,
} from "@/shared/constants";

@Component({
  selector: "app-loads-report",
  templateUrl: "./loads-report.html",
  imports: [
    ChartModule,
    TableModule,
    CurrencyPipe,
    DateRangePicker,
    DecimalPipe,
    SkeletonModule,
    TagModule,
    PageHeader,
    StatCard,
    DashboardCard,
  ],
})
export class LoadsReportComponent extends BaseReportComponent<LoadsReportDto> implements OnInit {
  // signals
  protected readonly chartData = signal<Record<string, unknown>>({});
  protected readonly trendChartData = signal<Record<string, unknown>>({});
  protected readonly performanceChartData = signal<Record<string, unknown>>({});
  protected readonly typeChartData = signal<Record<string, unknown>>({});

  // charts state
  protected pieOptions = getLoadsPieOptions();
  protected typeChartOptions = getLoadsTypeChartOptions();
  protected trendChartOptions = getLoadsTrendChartOptions();
  protected performanceChartOptions = getLoadsPerformanceChartOptions();

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override async query(params: ReportQueryParams): Promise<LoadsReportDto> {
    return this.api.invoke(getLoadsReport, {
      StartDate: params.startDate.toISOString(),
      EndDate: params.endDate?.toISOString(),
    });
  }

  protected override drawChart(result: LoadsReportDto): void {
    // ----- Pie (Loads by Status) -----
    const status = result.statusBreakdown ?? [];
    if (status.length) {
      const labels = status.map((s: StatusDto) => s.status);
      const data = status.map((s: StatusDto) => s.count);
      const colors = LOADS_CHART_PALETTE.slice(0, Math.max(1, labels.length));

      this.chartData.set({
        labels,
        datasets: [
          {
            label: "Loads by Status",
            data,
            backgroundColor: colors,
          },
        ],
      });
    }

    // ----- Bar (Loads by Type) -----
    const types = result.typeBreakdown ?? [];
    if (types.length > 0) {
      this.typeChartData.set({
        labels: types.map((t: TypeDto) => t.type),
        datasets: [
          {
            label: "Revenue",
            data: types.map((t: TypeDto) => t.totalRevenue),
            backgroundColor: "#42A5F5",
          },
        ],
      });
    }
    // ----- Line (Load Trends) -----
    const trends = result.loadTrends ?? [];
    if (trends.length > 0) {
      this.trendChartData.set({
        labels: trends.map((t: LoadTrendDto) => t.period),
        datasets: [
          {
            label: "Load Count",
            data: trends.map((t: LoadTrendDto) => t.loadCount),
            borderColor: "#2563eb",
            backgroundColor: "rgba(37, 99, 235, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Revenue",
            data: trends.map((t: LoadTrendDto) => t.revenue),
            borderColor: "#16a34a",
            backgroundColor: "rgba(22, 163, 74, 0.1)",
            tension: 0.4,
            yAxisID: "y1",
          },
        ],
      });
    }

    // ----- Performance Metrics -----
    const metrics = result.performanceMetrics ?? [];
    if (metrics.length > 0) {
      this.performanceChartData.set({
        labels: metrics.map((m: LoadPerformanceDto) => m.metric),
        datasets: [
          {
            label: "Value",
            data: metrics.map((m: LoadPerformanceDto) => m.value),
            backgroundColor: metrics.map((m: LoadPerformanceDto) =>
              (m.trend ?? 0) >= 0 ? "#16a34a" : "#ef4444",
            ),
            borderColor: metrics.map((m: LoadPerformanceDto) =>
              (m.trend ?? 0) >= 0 ? "#16a34a" : "#ef4444",
            ),
            borderWidth: 1,
          },
        ],
      });
    }
  }

  protected getStatusSeverity(status: string): Tag["severity"] {
    switch (status.toLowerCase()) {
      case "delivered":
        return "success";
      case "in_transit":
        return "info";
      case "picked_up":
        return "warn";
      case "cancelled":
        return "danger";
      default:
        return "secondary";
    }
  }
}
