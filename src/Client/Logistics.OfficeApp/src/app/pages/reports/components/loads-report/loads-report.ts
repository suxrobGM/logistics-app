import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, OnInit, signal } from "@angular/core";
import { Card } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { Observable } from "rxjs";
import { Result } from "@/core/api/models";
import { LoadsReportDto } from "@/core/api/models/report/loads-report.dto";
import { RangeCalendar } from "@/shared/components";
import {
  BaseReportComponent,
  ReportQueryParams,
} from "@/shared/components/base-report/base-report";
import {
  LOADS_CHART_PALETTE,
  LOADS_PERFORMANCE_CHART_OPTIONS,
  LOADS_PIE_OPTIONS,
  LOADS_TREND_CHART_OPTIONS,
  LOADS_TYPE_CHART_OPTIONS,
} from "@/shared/constants/loads-chart.options";

@Component({
  selector: "app-loads-report",
  templateUrl: "./loads-report.html",
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
export class LoadsReportComponent extends BaseReportComponent<LoadsReportDto> implements OnInit {
  // signals
  protected readonly chartData = signal<any>({});
  protected readonly trendChartData = signal<any>({});
  protected readonly performanceChartData = signal<any>({});
  protected readonly typeChartData = signal<any>({});

  // charts state
  protected pieOptions: any = LOADS_PIE_OPTIONS;
  protected typeChartOptions: any = LOADS_TYPE_CHART_OPTIONS;
  protected trendChartOptions: any = LOADS_TREND_CHART_OPTIONS;
  protected performanceChartOptions: any = LOADS_PERFORMANCE_CHART_OPTIONS;

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override query(params: ReportQueryParams): Observable<Result<LoadsReportDto>> {
    return this.apiService.reportApi.getLoadsReport({
      startDate: params.startDate,
      endDate: params.endDate,
    });
  }

  protected override drawChart(result: LoadsReportDto): void {
    // ----- Pie (Loads by Status) -----
    const status = result.statusBreakdown ?? [];
    if (status.length) {
      const labels = status.map((s) => s.status);
      const data = status.map((s) => s.count);
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
        labels: types.map((t) => t.type),
        datasets: [
          {
            label: "Revenue",
            data: types.map((t) => t.totalRevenue),
            backgroundColor: "#42A5F5",
          },
        ],
      });
    }
    // ----- Line (Load Trends) -----
    const trends = result.loadTrends ?? [];
    if (trends.length > 0) {
      this.trendChartData.set({
        labels: trends.map((t) => t.period),
        datasets: [
          {
            label: "Load Count",
            data: trends.map((t) => t.loadCount),
            borderColor: "#2563eb",
            backgroundColor: "rgba(37, 99, 235, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Revenue",
            data: trends.map((t) => t.revenue),
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
        labels: metrics.map((m) => m.metric),
        datasets: [
          {
            label: "Value",
            data: metrics.map((m) => m.value),
            backgroundColor: metrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderColor: metrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderWidth: 1,
          },
        ],
      });
    }
  }

  protected getStatusSeverity(status: string): string {
    switch (status.toLowerCase()) {
      case "delivered":
        return "success";
      case "in_transit":
        return "info";
      case "picked_up":
        return "warning";
      case "cancelled":
        return "danger";
      default:
        return "secondary";
    }
  }
}
