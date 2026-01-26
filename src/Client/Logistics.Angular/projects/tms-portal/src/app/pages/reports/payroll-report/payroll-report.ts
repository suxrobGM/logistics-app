import { CurrencyPipe, PercentPipe } from "@angular/common";
import { Component, type OnInit, computed, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { getPayrollReport } from "@logistics/shared/api";
import type {
  PayrollReportDto,
  PayrollTrendDto,
  SalaryTypeBreakdownDto,
} from "@logistics/shared/api";
import { salaryTypeOptions } from "@logistics/shared/api/enums";
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
import {
  PAYROLL_STATUS_LABELS,
  SALARY_TYPE_COLORS,
  getPayrollStatusChartOptions,
  getPayrollTrendChartOptions,
  getSalaryTypeChartOptions,
} from "@/shared/constants";

// Status chart colors - aligned with PAYROLL_STATUS_LABELS order
const STATUS_COLORS = {
  background: [
    "#64748b", // Draft - slate
    "#f59e0b", // Pending Approval - amber
    "#22c55e", // Approved - green
    "#ef4444", // Rejected - red
    "#3b82f6", // Partially Paid - blue
    "#10b981", // Paid - emerald
  ],
  hover: ["#475569", "#d97706", "#16a34a", "#dc2626", "#2563eb", "#059669"],
};
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-payroll-report",
  templateUrl: "./payroll-report.html",
  imports: [
    TableModule,
    ChartModule,
    DateRangePicker,
    CurrencyPipe,
    PercentPipe,
    SkeletonModule,
    TagModule,
    PageHeader,
    StatCard,
    DashboardCard,
    RouterModule,
  ],
})
export class PayrollReportComponent
  extends BaseReportComponent<PayrollReportDto>
  implements OnInit
{
  protected readonly statusChartData = signal<Record<string, unknown>>({});
  protected readonly trendChartData = signal<Record<string, unknown>>({});
  protected readonly salaryTypeChartData = signal<Record<string, unknown>>({});

  protected readonly hasStatusChartData = computed(
    () => Object.keys(this.statusChartData()).length > 0,
  );
  protected readonly hasTrendChartData = computed(
    () => Object.keys(this.trendChartData()).length > 0,
  );
  protected readonly hasSalaryTypeChartData = computed(
    () => Object.keys(this.salaryTypeChartData()).length > 0,
  );

  protected readonly statusChartOptions = getPayrollStatusChartOptions();
  protected readonly trendChartOptions = getPayrollTrendChartOptions();
  protected readonly salaryTypeChartOptions = getSalaryTypeChartOptions();

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override async query(params: ReportQueryParams): Promise<PayrollReportDto> {
    return this.api.invoke(getPayrollReport, {
      StartDate: params.startDate.toISOString(),
      EndDate: params.endDate?.toISOString(),
    });
  }

  protected override drawChart(result: PayrollReportDto): void {
    // Status breakdown pie chart
    const statusBreakdown = result.statusBreakdown;
    if (statusBreakdown) {
      const statusData = [
        statusBreakdown.draft ?? 0,
        statusBreakdown.pendingApproval ?? 0,
        statusBreakdown.approved ?? 0,
        statusBreakdown.rejected ?? 0,
        statusBreakdown.partiallyPaid ?? 0,
        statusBreakdown.paid ?? 0,
      ];

      const hasData = statusData.some((v) => v > 0);
      if (hasData) {
        this.statusChartData.set({
          labels: PAYROLL_STATUS_LABELS,
          datasets: [
            {
              data: statusData,
              backgroundColor: STATUS_COLORS.background,
              hoverBackgroundColor: STATUS_COLORS.hover,
            },
          ],
        });
      }
    }

    // Payroll trends line chart
    const trends = result.payrollTrends ?? [];
    if (trends.length > 0) {
      this.trendChartData.set({
        labels: trends.map((t: PayrollTrendDto) => t.period),
        datasets: [
          {
            label: "Total Amount",
            data: trends.map((t: PayrollTrendDto) => t.totalAmount),
            borderColor: "#3b82f6",
            backgroundColor: "rgba(59, 130, 246, 0.1)",
            tension: 0.4,
            fill: true,
          },
          {
            label: "Paid Amount",
            data: trends.map((t: PayrollTrendDto) => t.paidAmount),
            borderColor: "#22c55e",
            backgroundColor: "rgba(34, 197, 94, 0.1)",
            tension: 0.4,
            fill: true,
          },
        ],
      });
    }

    // Salary type breakdown bar chart
    const salaryTypes = result.salaryTypeBreakdown ?? [];
    if (salaryTypes.length > 0) {
      this.salaryTypeChartData.set({
        labels: salaryTypes.map((s: SalaryTypeBreakdownDto) =>
          this.getSalaryTypeLabel(s.salaryType),
        ),
        datasets: [
          {
            label: "Total Amount",
            data: salaryTypes.map((s: SalaryTypeBreakdownDto) => s.totalAmount),
            backgroundColor: salaryTypes.map(
              (s: SalaryTypeBreakdownDto) =>
                SALARY_TYPE_COLORS[s.salaryType ?? "none"] ?? "#64748b",
            ),
          },
        ],
      });
    }
  }

  protected getSalaryTypeLabel(salaryType?: string | null): string {
    return salaryTypeOptions.find((opt) => opt.value === salaryType)?.label ?? salaryType ?? "N/A";
  }

  protected getSalaryTypeColor(salaryType?: string | null): string {
    return SALARY_TYPE_COLORS[salaryType ?? "none"] ?? "#64748b";
  }

  protected getPercentageClass(percentage?: number | null): string {
    const value = percentage ?? 0;
    if (value >= 30) return "bg-green-600/10 text-green-600";
    if (value >= 15) return "bg-blue-600/10 text-blue-600";
    return "bg-slate-600/10 text-slate-600";
  }

  protected getEmployeeInitials(name?: string | null): string {
    return Converters.getInitials(name);
  }
}
