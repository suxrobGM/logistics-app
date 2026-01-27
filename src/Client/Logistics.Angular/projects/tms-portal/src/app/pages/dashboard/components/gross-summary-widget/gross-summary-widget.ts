import { CurrencyPipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "app-gross-summary-widget",
  templateUrl: "./gross-summary-widget.html",
  imports: [CardModule, DividerModule, SkeletonModule, CurrencyPipe],
})
export class GrossSummaryWidgetComponent {
  public readonly thisWeekGross = input(0);
  public readonly lastWeekGross = input(0);
  public readonly thisMonthGross = input(0);
  public readonly lastMonthGross = input(0);
  public readonly lastThreeMonthsGross = input(0);
  public readonly lastYearGross = input(0);
  public readonly totalGross = input(0);
  public readonly isLoading = input(false);

  protected readonly weekOverWeekChange = computed(() => {
    const thisWeek = this.thisWeekGross();
    const lastWeek = this.lastWeekGross();
    if (lastWeek === 0) return null;
    const change = ((thisWeek - lastWeek) / lastWeek) * 100;
    return {
      value: change,
      formatted: `${change >= 0 ? "+" : ""}${change.toFixed(1)}%`,
      isPositive: change >= 0,
    };
  });

  protected readonly monthOverMonthChange = computed(() => {
    const thisMonth = this.thisMonthGross();
    const lastMonth = this.lastMonthGross();
    if (lastMonth === 0) return null;
    const change = ((thisMonth - lastMonth) / lastMonth) * 100;
    return {
      value: change,
      formatted: `${change >= 0 ? "+" : ""}${change.toFixed(1)}%`,
      isPositive: change >= 0,
    };
  });
}
