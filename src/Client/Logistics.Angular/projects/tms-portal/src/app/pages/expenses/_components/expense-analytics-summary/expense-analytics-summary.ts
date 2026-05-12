import { Component, input } from "@angular/core";
import { CurrencyFormatPipe } from "@logistics/shared";
import { type ExpenseStatsDto } from "@logistics/shared/api";
import { Grid, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-expense-analytics-summary",
  templateUrl: "./expense-analytics-summary.html",
  imports: [CardModule, CurrencyFormatPipe, Grid, Typography],
})
export class ExpenseAnalyticsSummary {
  public readonly stats = input.required<ExpenseStatsDto>();
}
