import { Component, OnInit, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { CommonModule } from "@angular/common";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { Result } from "@/core/api/models";

import { RangeCalendar } from "@/shared/components";
import { Observable } from "rxjs";
import { ChartModule } from "primeng/chart";
import { BaseReportComponent, ReportQueryParams } from "@/shared/components/base-report/base-report";
import { FinancialsReportDto } from "@/core/api/models/report/financials-report.dto";

// =======================
// Chart Constants
// =======================
const CHART_LABELS = ["Fully Paid", "Partially Paid", "Unpaid"];
const CHART_BACKGROUND_COLORS = ["#4caf50", "#ff9800", "#f44336"];
const CHART_HOVER_BACKGROUND_COLORS = ["#66bb6a", "#ffb74d", "#e57373"];

const INITIAL_CHART_DATA = {
  labels: CHART_LABELS,
  datasets: [
    {
      data: [1, 1, 1],
      backgroundColor: CHART_BACKGROUND_COLORS,
      hoverBackgroundColor: CHART_HOVER_BACKGROUND_COLORS,
    },
  ],
};

const CHART_OPTIONS = {
  responsive: true,
  plugins: {
    legend: {
      position: "bottom",
    },
  },
};

@Component({
  selector: "app-financials-report",
  templateUrl: "./financials-report.html",
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    TableModule,
    ChartModule,
    RangeCalendar,
  ],
})
export class FinancialsReportComponent
  extends BaseReportComponent<FinancialsReportDto>
  implements OnInit
{
  protected readonly chartOptions = CHART_OPTIONS;
  protected readonly chartData = signal<Record<string, unknown>>(
    INITIAL_CHART_DATA
  );

  ngOnInit(): void {
    this.fetch({ startDate: this.startDate(), endDate: this.endDate() });
  }

  protected override query(
    params: ReportQueryParams
  ): Observable<Result<FinancialsReportDto>> {
    return this.apiService.reportApi.getFinancialsReport({
      startDate: params.startDate,
      endDate: params.endDate,
    });
  }

  protected override drawChart(result: FinancialsReportDto): void {
    const hasData =
      result.fullyPaidInvoices +
      result.partiallyPaidInvoices +
      result.unpaidInvoices;

    if (hasData > 0) {
      this.chartData.set({
        labels: CHART_LABELS,
        datasets: [
          {
            data: [
              result.fullyPaidInvoices,
              result.partiallyPaidInvoices,
              result.unpaidInvoices,
            ],
            backgroundColor: CHART_BACKGROUND_COLORS,
            hoverBackgroundColor: CHART_HOVER_BACKGROUND_COLORS,
          },
        ],
      });
    } else {
      if (this.chartData() !== INITIAL_CHART_DATA) {
        this.chartData.set(INITIAL_CHART_DATA);
      }
    }
  }
}
