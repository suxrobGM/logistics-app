import { CommonModule } from "@angular/common";
import { Component, input } from "@angular/core";
import { CurrencyFormatPipe } from "@logistics/shared";
import { type ExpenseDto } from "@logistics/shared/api";
import { Card, Divider, InfoRow, Typography } from "@logistics/shared/ui";
import { ExpenseStatusTag, ExpenseTypeTag } from "@/shared/components/tags";
import { getCategoryLabel } from "../expense.constants";

@Component({
  selector: "app-expense-detail-info-card",
  templateUrl: "./expense-detail-info-card.html",
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    Divider,
    ExpenseStatusTag,
    ExpenseTypeTag,
    InfoRow,
    Typography,
  ],
})
export class ExpenseDetailInfoCard {
  public readonly expense = input.required<ExpenseDto>();

  protected categoryLabel(): string {
    return getCategoryLabel(this.expense());
  }
}
