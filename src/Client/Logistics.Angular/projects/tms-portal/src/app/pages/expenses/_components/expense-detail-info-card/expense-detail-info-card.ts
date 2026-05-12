import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import { CurrencyFormatPipe } from "@logistics/shared";
import { type ExpenseDto } from "@logistics/shared/api";
import { Typography } from "@logistics/shared/components";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ExpenseStatusTag, ExpenseTypeTag } from "@/shared/components/tags";
import { ExpenseDetailRow } from "../expense-detail-row/expense-detail-row";
import { getCategoryLabel } from "../expense.constants";

@Component({
  selector: "app-expense-detail-info-card",
  templateUrl: "./expense-detail-info-card.html",
  imports: [
    CommonModule,
    CardModule,
    DividerModule,
    Typography,
    CurrencyFormatPipe,
    ExpenseStatusTag,
    ExpenseTypeTag,
    ExpenseDetailRow,
  ],
})
export class ExpenseDetailInfoCard {
  public readonly expense = input.required<ExpenseDto>();

  protected categoryLabel(): string {
    return getCategoryLabel(this.expense());
  }
}
