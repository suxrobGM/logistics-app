import { Component, input } from "@angular/core";
import { Card, Grid, Typography } from "@logistics/shared/ui";
import { ChartModule } from "primeng/chart";
import { chartOptions, lineChartOptions } from "../expense-analytics.utils";

@Component({
  selector: "app-expense-analytics-charts",
  templateUrl: "./expense-analytics-charts.html",
  imports: [Card, ChartModule, Grid, Typography],
})
export class ExpenseAnalyticsCharts {
  public readonly typeData = input<unknown>(null);
  public readonly companyCategoryData = input<unknown>(null);
  public readonly truckCategoryData = input<unknown>(null);
  public readonly monthlyTrendData = input<unknown>(null);

  protected readonly chartOptions = chartOptions;
  protected readonly lineChartOptions = lineChartOptions;
}
