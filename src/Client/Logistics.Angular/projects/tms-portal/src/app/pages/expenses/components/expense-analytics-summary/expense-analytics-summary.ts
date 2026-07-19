import { Component, input } from "@angular/core";
import { CurrencyFormatPipe } from "@logistics/shared";
import { type ExpenseStatsDto } from "@logistics/shared/api";
import { Card, Grid, Typography } from "@logistics/shared/ui";

@Component({
  selector: "app-expense-analytics-summary",
  templateUrl: "./expense-analytics-summary.html",
  imports: [Card, CurrencyFormatPipe, Grid, Typography],
})
export class ExpenseAnalyticsSummary {
  public readonly stats = input.required<ExpenseStatsDto>();
}
