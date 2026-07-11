import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import { type ExpenseDto } from "@logistics/shared/api";
import { Card, Grid, Typography } from "@logistics/shared/ui";

@Component({
  selector: "app-expense-detail-audit-card",
  templateUrl: "./expense-detail-audit-card.html",
  imports: [Card, CommonModule, Grid, Typography],
})
export class ExpenseDetailAuditCard {
  public readonly expense = input.required<ExpenseDto>();
}
