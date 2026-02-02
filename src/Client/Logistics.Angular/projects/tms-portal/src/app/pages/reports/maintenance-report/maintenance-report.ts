import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, computed, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { getMaintenanceReport } from "@logistics/shared/api";
import type {
  MaintenanceReportDto,
  MaintenanceTrendDto,
  MaintenanceTypeBreakdownDto,
  MaintenanceVendorBreakdownDto,
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

// Maintenance type colors
const TYPE_COLORS = [
  "#3b82f6",
  "#22c55e",
  "#f59e0b",
  "#ef4444",
  "#8b5cf6",
  "#ec4899",
  "#06b6d4",
  "#84cc16",
  "#64748b",
  "#14b8a6",
];

@Component({
  selector: "app-maintenance-report",
  templateUrl: "./maintenance-report.html",
  imports: [
    TableModule,
    ChartModule,
    DateRangePicker,
    CurrencyPipe,
    DatePipe,
    SkeletonModule,
    TagModule,
    PageHeader,
    StatCard,
    DashboardCard,
    RouterModule,
  ],
})
export class MaintenanceReportComponent extends BaseReportComponent<MaintenanceReportDto> implements OnInit {
  protected readonly costTrendChartData = signal<Record<string, unknown>>({});
  protected readonly serviceTypeChartData = signal<Record<string, unknown>>({});
  protected readonly vendorChartData = signal<Record<string, unknown>>({});
  protected readonly trendDataPointCount = signal(0);

  protected readonly hasCostTrendData = computed(() => Object.keys(this.costTrendChartData()).length > 0);
  protected readonly hasServiceTypeData = computed(() => Object.keys(this.serviceTypeChartData()).length > 0);
  protected readonly hasVendorData = computed(() => Object.keys(this.vendorChartData()).length > 0);

  // Use bar chart for small datasets (3 or fewer points)
  protected readonly trendChartType = computed(() => (this.trendDataPointCount() <= 3 ? "bar" : "line"));

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
      y: {
        beginAtZero: true,
        ticks: {
          callback: (value: number) => "$" + value.toLocaleString(),
        },
      },
    },
    maintainAspectRatio: false,
  };

  protected readonly trendChartOptions = computed(() => {
    const pointCount = this.trendDataPointCount();
    const isBarChart = pointCount <= 3;

    return {
      plugins: {
        legend: { position: "bottom" as const },
      },
      layout: {
        padding: { left: 10 },
      },
      scales: {
        x: {
          title: {
            display: true,
            text: "Month",
            font: { weight: "bold" as const },
          },
        },
        y: {
          beginAtZero: true,
          title: {
            display: true,
            text: "Cost ($)",
            font: { weight: "bold" as const },
          },
          ticks: {
            callback: (value: number) => "$" + value.toLocaleString(),
          },
        },
      },
      maintainAspectRatio: false,
      // For line charts, show larger points when there are few data points
      ...(!isBarChart && {
        elements: {
          point: {
            radius: pointCount <= 6 ? 6 : 3,
            hoverRadius: pointCount <= 6 ? 8 : 5,
          },
        },
      }),
    };
  });

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override async query(params: ReportQueryParams): Promise<MaintenanceReportDto> {
    return this.api.invoke(getMaintenanceReport, {
      StartDate: params.startDate.toISOString(),
      EndDate: params.endDate?.toISOString(),
    });
  }

  protected override drawChart(result: MaintenanceReportDto): void {
    // Cost trends chart (bar for small datasets, line for larger)
    const costTrends = result.costTrends ?? [];
    if (costTrends.length > 0) {
      const pointCount = costTrends.length;
      this.trendDataPointCount.set(pointCount);

      // Use bar chart styling for small datasets, line chart for larger ones
      const isBarChart = pointCount <= 3;
      // Reduce tension for small datasets to avoid misleading curves
      const tension = pointCount <= 4 ? 0 : 0.4;

      this.costTrendChartData.set({
        labels: costTrends.map((t: MaintenanceTrendDto) => t.period),
        datasets: [
          {
            label: "Total Cost",
            data: costTrends.map((t: MaintenanceTrendDto) => t.totalCost),
            borderColor: "#3b82f6",
            backgroundColor: isBarChart ? "#3b82f6" : "rgba(59, 130, 246, 0.1)",
            tension,
            fill: !isBarChart,
            pointBackgroundColor: "#3b82f6",
          },
          {
            label: "Labor Cost",
            data: costTrends.map((t: MaintenanceTrendDto) => t.laborCost),
            borderColor: "#22c55e",
            backgroundColor: isBarChart ? "#22c55e" : "rgba(34, 197, 94, 0.1)",
            tension,
            fill: !isBarChart,
            pointBackgroundColor: "#22c55e",
          },
          {
            label: "Parts Cost",
            data: costTrends.map((t: MaintenanceTrendDto) => t.partsCost),
            borderColor: "#f59e0b",
            backgroundColor: isBarChart ? "#f59e0b" : "rgba(245, 158, 11, 0.1)",
            tension,
            fill: !isBarChart,
            pointBackgroundColor: "#f59e0b",
          },
        ],
      });
    } else {
      this.trendDataPointCount.set(0);
    }

    // Service type breakdown bar chart
    const serviceTypes = result.byServiceType ?? [];
    if (serviceTypes.length > 0) {
      this.serviceTypeChartData.set({
        labels: serviceTypes.map(
          (s: MaintenanceTypeBreakdownDto) => s.maintenanceTypeDisplay || s.maintenanceType,
        ),
        datasets: [
          {
            label: "Total Cost",
            data: serviceTypes.map((s: MaintenanceTypeBreakdownDto) => s.totalCost),
            backgroundColor: TYPE_COLORS.slice(0, serviceTypes.length),
          },
        ],
      });
    }

    // Vendor breakdown pie chart
    const vendors = result.byVendor ?? [];
    if (vendors.length > 0) {
      this.vendorChartData.set({
        labels: vendors.map((v: MaintenanceVendorBreakdownDto) => v.vendorName || "Unknown"),
        datasets: [
          {
            data: vendors.map((v: MaintenanceVendorBreakdownDto) => v.totalCost),
            backgroundColor: TYPE_COLORS.slice(0, vendors.length),
          },
        ],
      });
    }
  }

  protected getOverdueClass(overdueCount?: number): string {
    const value = overdueCount ?? 0;
    if (value >= 5) return "text-red-600";
    if (value >= 1) return "text-amber-600";
    return "text-green-600";
  }
}
