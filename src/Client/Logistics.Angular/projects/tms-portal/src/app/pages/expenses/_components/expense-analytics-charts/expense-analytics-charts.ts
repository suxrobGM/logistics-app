import { Component, input } from "@angular/core";
import { Grid, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { ChartModule } from "primeng/chart";
import { chartOptions, lineChartOptions } from "../expense-analytics.utils";

@Component({
  selector: "app-expense-analytics-charts",
  templateUrl: "./expense-analytics-charts.html",
  imports: [CardModule, ChartModule, Grid, Typography],
})
export class ExpenseAnalyticsCharts {
  public readonly typeData = input<unknown>(null);
  public readonly companyCategoryData = input<unknown>(null);
  public readonly truckCategoryData = input<unknown>(null);
  public readonly monthlyTrendData = input<unknown>(null);

  protected readonly chartOptions = chartOptions;
  protected readonly lineChartOptions = lineChartOptions;
}
