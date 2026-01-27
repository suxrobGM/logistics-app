import { CurrencyPipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { BadgeModule } from "primeng/badge";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

@Component({
  selector: "app-financial-health-widget",
  templateUrl: "./financial-health-widget.html",
  imports: [CardModule, BadgeModule, DividerModule, RouterLink, CurrencyPipe, SkeletonModule],
})
export class FinancialHealthWidgetComponent {
  public readonly outstandingInvoiceTotal = input<number>(0);
  public readonly paymentsReceivedThisWeek = input<number>(0);
  public readonly overdueInvoiceCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);
}
