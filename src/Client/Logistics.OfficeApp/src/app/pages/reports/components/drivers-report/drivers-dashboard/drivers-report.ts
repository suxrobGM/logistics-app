import { CurrencyPipe, DecimalPipe, PercentPipe } from "@angular/common";
import { Component, OnInit, inject, signal } from "@angular/core";
import { Card } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { Observable } from "rxjs";
import { ApiService } from "@/core/api";
import { Result } from "@/core/api/models";
import { DriverDashboardDto } from "@/core/api/models/report/drivers-report.dto";
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
  standalone: true,
})
export class DriversReportComponent
  extends BaseReportComponent<DriverDashboardDto>
  implements OnInit
{
  protected override readonly apiService = inject(ApiService);

  protected readonly trendChartData = signal<any>({});
  protected readonly efficiencyChartData = signal<any>({});
  protected readonly performanceChartData = signal<any>({});

  protected trendChartOptions: any = DRIVERS_TREND_CHART_OPTIONS;
  protected efficiencyChartOptions: any = DRIVERS_EFFICIENCY_CHART_OPTIONS;
  protected performanceChartOptions: any = DRIVERS_PERFORMANCE_CHART_OPTIONS;

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override query(params: ReportQueryParams): Observable<Result<DriverDashboardDto>> {
    return this.apiService.reportApi.getDriverDashboard({
      startDate: params.startDate,
      endDate: params.endDate,
    });
  }

  protected override drawChart(result: DriverDashboardDto): void {
    // Driver Trends Chart
    const trends = result.driverTrends ?? [];
    if (trends.length > 0) {
      this.trendChartData.set({
        labels: trends.map((t) => t.period),
        datasets: [
          {
            label: "Active Drivers",
            data: trends.map((t) => t.activeDrivers),
            borderColor: "#2563eb",
            backgroundColor: "rgba(37, 99, 235, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Loads Delivered",
            data: trends.map((t) => t.loadsDelivered),
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
        labels: efficiencyMetrics.map((m) => m.metric),
        datasets: [
          {
            label: "Value",
            data: efficiencyMetrics.map((m) => m.value),
            backgroundColor: efficiencyMetrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderColor: efficiencyMetrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderWidth: 1,
          },
        ],
      });
    }

    // Top Performers Chart
    const topPerformers = result.topPerformers ?? [];
    if (topPerformers.length > 0) {
      this.performanceChartData.set({
        labels: topPerformers.map((p) => p.driverName),
        datasets: [
          {
            label: "Earnings",
            data: topPerformers.map((p) => p.earnings),
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

  protected getPerformanceSeverity(efficiency: number): string {
    if (efficiency >= 2.0) return "success";
    if (efficiency >= 1.5) return "info";
    if (efficiency >= 1.0) return "warning";
    return "danger";
  }
}
