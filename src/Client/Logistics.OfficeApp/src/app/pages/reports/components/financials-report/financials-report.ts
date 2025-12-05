import { CommonModule } from "@angular/common";
import { CurrencyPipe, DecimalPipe } from "@angular/common";
import { Component, OnInit, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { InputTextModule } from "primeng/inputtext";
import { SkeletonModule } from "primeng/skeleton";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { Observable } from "rxjs";
import { Result } from "@/core/api/models";
import { FinancialsReportDto } from "@/core/api/models/report/financials-report.dto";
import { RangeCalendar } from "@/shared/components";
import {
  BaseReportComponent,
  ReportQueryParams,
} from "@/shared/components/base-report/base-report";
import {
  FINANCIALS_CHART_BACKGROUND_COLORS,
  FINANCIALS_CHART_HOVER_BACKGROUND_COLORS,
  FINANCIALS_CHART_LABELS,
  FINANCIAL_METRICS_CHART_OPTIONS,
  INVOICE_STATUS_CHART_OPTIONS,
  REVENUE_TREND_CHART_OPTIONS,
} from "@/shared/constants";

@Component({
  selector: "app-financials-report",
  templateUrl: "./financials-report.html",
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    TableModule,
    ChartModule,
    RangeCalendar,
    CurrencyPipe,
    DecimalPipe,
    SkeletonModule,
    TagModule,
  ],
})
export class FinancialsReportComponent
  extends BaseReportComponent<FinancialsReportDto>
  implements OnInit
{
  protected readonly invoiceStatusChartData = signal<Record<string, unknown>>({});
  protected readonly revenueTrendChartData = signal<Record<string, unknown>>({});
  protected readonly financialMetricsChartData = signal<Record<string, unknown>>({});

  protected readonly invoiceStatusChartOptions = INVOICE_STATUS_CHART_OPTIONS;
  protected revenueTrendChartOptions = REVENUE_TREND_CHART_OPTIONS;
  protected financialMetricsChartOptions = FINANCIAL_METRICS_CHART_OPTIONS;

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override query(params: ReportQueryParams): Observable<Result<FinancialsReportDto>> {
    return this.apiService.reportApi.getFinancialsReport({
      startDate: params.startDate,
      endDate: params.endDate,
    });
  }

  protected override drawChart(result: FinancialsReportDto): void {
    const hasData = result.fullyPaidInvoices + result.partiallyPaidInvoices + result.unpaidInvoices;

    if (hasData > 0) {
      this.invoiceStatusChartData.set({
        labels: FINANCIALS_CHART_LABELS,
        datasets: [
          {
            data: [result.fullyPaidInvoices, result.partiallyPaidInvoices, result.unpaidInvoices],
            backgroundColor: FINANCIALS_CHART_BACKGROUND_COLORS,
            hoverBackgroundColor: FINANCIALS_CHART_HOVER_BACKGROUND_COLORS,
          },
        ],
      });
    }

    // Revenue Trends Chart
    const revenueTrends = result.revenueTrends ?? [];
    if (revenueTrends.length > 0) {
      this.revenueTrendChartData.set({
        labels: revenueTrends.map((t) => t.period),
        datasets: [
          {
            label: "Revenue",
            data: revenueTrends.map((t) => t.revenue),
            borderColor: "#2563eb",
            backgroundColor: "rgba(37, 99, 235, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Profit",
            data: revenueTrends.map((t) => t.profit),
            borderColor: "#16a34a",
            backgroundColor: "rgba(22, 163, 74, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
          {
            label: "Expenses",
            data: revenueTrends.map((t) => t.expenses),
            borderColor: "#ef4444",
            backgroundColor: "rgba(239, 68, 68, 0.1)",
            tension: 0.4,
            yAxisID: "y",
          },
        ],
      });
    }

    // Financial Metrics Chart
    const financialMetrics = result.financialMetrics ?? [];
    if (financialMetrics.length > 0) {
      this.financialMetricsChartData.set({
        labels: financialMetrics.map((m) => m.metric),
        datasets: [
          {
            label: "Value",
            data: financialMetrics.map((m) => m.value),
            backgroundColor: financialMetrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderColor: financialMetrics.map((m) => (m.trend >= 0 ? "#16a34a" : "#ef4444")),
            borderWidth: 1,
          },
        ],
      });
    }
  }

  protected getMetricSeverity(trend: number): Tag["severity"] {
    return trend >= 0 ? "success" : "danger";
  }

  protected getCategorySeverity(category: string): Tag["severity"] {
    switch (category.toLowerCase()) {
      case "revenue":
        return "success";
      case "performance":
        return "info";
      case "risk":
        return "danger";
      default:
        return "secondary";
    }
  }
}
