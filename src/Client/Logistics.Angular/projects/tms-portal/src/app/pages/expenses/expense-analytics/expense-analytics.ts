import { CommonModule } from "@angular/common";
import { Component, inject, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { RouterModule } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import { Card, Icon, Spinner, Typography, UiButton, UiDataTable } from "@logistics/shared/ui";
import { DatePicker } from "primeng/datepicker";
import { PageHeader } from "@/shared/components";
import { ExpenseAnalyticsCharts, ExpenseAnalyticsSummary } from "../_components";
import { ExpenseAnalyticsStore } from "../store/expense-analytics.store";

const RANK_BADGE_CLASSES: Record<number, string> = {
  0: "bg-warning/20 text-warning",
  1: "bg-active text-subtle-foreground",
  2: "bg-warning/20 text-warning",
};

@Component({
  selector: "app-expense-analytics",
  templateUrl: "./expense-analytics.html",
  providers: [ExpenseAnalyticsStore],
  imports: [
    Card,
    CommonModule,
    CurrencyFormatPipe,
    DatePicker,
    ExpenseAnalyticsCharts,
    ExpenseAnalyticsSummary,
    FormsModule,
    Icon,
    PageHeader,
    RouterModule,
    Spinner,
    Typography,
    UiButton,
    UiDataTable,
  ],
})
export class ExpenseAnalyticsPage implements OnInit {
  protected readonly store = inject(ExpenseAnalyticsStore);

  ngOnInit(): void {
    this.store.load();
  }

  onDateChange(): void {
    this.store.load();
  }

  protected rankBadgeClass(index: number): string {
    return RANK_BADGE_CLASSES[index] ?? "bg-hover text-subtle-foreground";
  }
}
