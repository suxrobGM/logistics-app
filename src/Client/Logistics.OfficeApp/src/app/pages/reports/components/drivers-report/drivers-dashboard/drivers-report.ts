import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, OnInit, signal } from "@angular/core";
import { Card } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { getDriverDashboard$Json } from "@/core/api";
import {
  DriverDashboardDto,
  DriverEfficiencyDto,
  DriverPerformanceDto,
  DriverTrendDto,
  Result,
} from "@/core/api/models";
import { RangeCalendar } from "@/shared/components";
import {
  BaseReportComponent,
  ReportQueryParams,
} from "@/shared/components/base-report/base-report";
import {
  DRIVERS_CHART_PALETTE,
  DRIVERS_EFFICIENCY_CHART_OPTIONS,
  DRIVERS_PERFORMANCE_CHART_OPTIONS,
  DRIVERS_TREND_CHART_OPTIONS,
} from "@/shared/constants/drivers-chart.options";

@Component({
  selector: "app-drivers-report",
  templateUrl: "./drivers-report.html",
  imports: [
    Card,
    ChartModule,
    TableModule,
    CurrencyPipe,
    RangeCalendar,
    DecimalPipe,
    SkeletonModule,
    TagModule,
  ],
})
export class DriversReportComponent
  extends BaseReportComponent<DriverDashboardDto>
  implements OnInit
{
  protected readonly trendChartData = signal<Record<string, unknown>>({});
  protected readonly efficiencyChartData = signal<Record<string, unknown>>({});
  protected readonly performanceChartData = signal<Record<string, unknown>>({});

  protected trendChartOptions = DRIVERS_TREND_CHART_OPTIONS;
  protected efficiencyChartOptions = DRIVERS_EFFICIENCY_CHART_OPTIONS;
  protected performanceChartOptions = DRIVERS_PERFORMANCE_CHART_OPTIONS;

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override async query(params: ReportQueryParams): Promise<Result<DriverDashboardDto>> {
    return this.api.invoke(getDriverDashboard$Json, {
      StartDate: params.startDate.toISOString(),
      EndDate: params.endDate?.toISOString(),
    });
  }

  protected override drawChart(result: DriverDashboardDto): void {
    // Driver Trends Chart
    const trends = result.driverTrends ?? [];
    if (trends.length > 0) {
      this.trendChartData.set({
        labels: trends.map((t: DriverTrendDto) => t.period),
        datasets: [
          {
            label: "Active Drivers",
            data: trends.map((t: DriverTrendDto) => t.activeDrivers),
            borderColor: "#2563eb",
            backgroundColor: "rgba(37, 99, 235, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Loads Delivered",
            data: trends.map((t: DriverTrendDto) => t.loadsDelivered),
            borderColor: "#16a34a",
            backgroundColor: "rgba(22, 163, 74, 0.1)",
            tension: 0.4,
            yAxisID: "y1",
          },
        ],
      });
    }

    // Efficiency Metrics Chart
    const efficiencyMetrics = result.efficiencyMetrics ?? [];
    if (efficiencyMetrics.length > 0) {
      this.efficiencyChartData.set({
        labels: efficiencyMetrics.map((m: DriverEfficiencyDto) => m.metric),
        datasets: [
          {
            label: "Value",
            data: efficiencyMetrics.map((m: DriverEfficiencyDto) => m.value),
            backgroundColor: efficiencyMetrics.map((m: DriverEfficiencyDto) =>
              (m.trend ?? 0) >= 0 ? "#16a34a" : "#ef4444",
            ),
            borderColor: efficiencyMetrics.map((m: DriverEfficiencyDto) =>
              (m.trend ?? 0) >= 0 ? "#16a34a" : "#ef4444",
            ),
            borderWidth: 1,
          },
        ],
      });
    }

    // Top Performers Chart
    const topPerformers = result.topPerformers ?? [];
    if (topPerformers.length > 0) {
      this.performanceChartData.set({
        labels: topPerformers.map((p: DriverPerformanceDto) => p.driverName),
        datasets: [
          {
            label: "Earnings",
            data: topPerformers.map((p: DriverPerformanceDto) => p.earnings),
            backgroundColor: DRIVERS_CHART_PALETTE.slice(0, topPerformers.length),
            borderColor: DRIVERS_CHART_PALETTE.slice(0, topPerformers.length),
            borderWidth: 1,
          },
        ],
      });
    }
  }

  protected getPerformanceLevel(efficiency: number): string {
    if (efficiency >= 2.0) return "Excellent";
    if (efficiency >= 1.5) return "Good";
    if (efficiency >= 1.0) return "Average";
    return "Below Average";
  }

  protected getPerformanceSeverity(efficiency: number): Tag["severity"] {
    if (efficiency >= 2.0) return "success";
    if (efficiency >= 1.5) return "info";
    if (efficiency >= 1.0) return "warn";
    return "danger";
  }
}
