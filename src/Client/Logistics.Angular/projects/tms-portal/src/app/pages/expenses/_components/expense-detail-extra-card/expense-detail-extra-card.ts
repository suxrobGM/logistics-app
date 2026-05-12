import { CommonModule } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { RouterModule } from "@angular/router";
import { LocalizationService } from "@logistics/shared";
import { type ExpenseDto } from "@logistics/shared/api";
import { Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ExpenseDetailRow } from "../expense-detail-row/expense-detail-row";
import { toShortVolumeUnit } from "../expense.constants";

@Component({
  selector: "app-expense-detail-extra-card",
  templateUrl: "./expense-detail-extra-card.html",
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    ButtonModule,
    DividerModule,
    Typography,
    ExpenseDetailRow,
  ],
})
export class ExpenseDetailExtraCard {
  private readonly localization = inject(LocalizationService);

  public readonly expense = input.required<ExpenseDto>();
  public readonly viewReceipt = output<void>();

  protected formatQuantity(): string {
    const e = this.expense();
    if (e.quantity == null) return "";
    return this.localization.formatVolume(e.quantity, toShortVolumeUnit(e.quantityUnit));
  }
}
