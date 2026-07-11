import { Component, input } from "@angular/core";
import { Card, Grid, Typography, UiChart } from "@logistics/shared/ui";
import { chartOptions, lineChartOptions } from "../expense-analytics.utils";

@Component({
  selector: "app-expense-analytics-charts",
  templateUrl: "./expense-analytics-charts.html",
  imports: [Card, Grid, Typography, UiChart],
})
export class ExpenseAnalyticsCharts {
  public readonly typeData = input<unknown>(null);
  public readonly companyCategoryData = input<unknown>(null);
  public readonly truckCategoryData = input<unknown>(null);
  public readonly monthlyTrendData = input<unknown>(null);

  protected readonly chartOptions = chartOptions;
  protected readonly lineChartOptions = lineChartOptions;
}
