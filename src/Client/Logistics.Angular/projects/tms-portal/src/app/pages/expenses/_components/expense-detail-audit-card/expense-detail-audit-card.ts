import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import { type ExpenseDto } from "@logistics/shared/api";
import { Grid, Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";

@Component({
  selector: "app-expense-detail-audit-card",
  templateUrl: "./expense-detail-audit-card.html",
  imports: [CommonModule, CardModule, Grid, Typography],
})
export class ExpenseDetailAuditCard {
  public readonly expense = input.required<ExpenseDto>();
}
