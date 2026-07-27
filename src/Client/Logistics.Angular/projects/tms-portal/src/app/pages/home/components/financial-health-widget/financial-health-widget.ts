import { Component, computed, inject, input } from "@angular/core";
import { Router, RouterLink } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import {
  Card,
  CountBadge,
  Divider,
  EmptyState,
  Icon,
  Skeleton,
  Stack,
  Typography,
} from "@logistics/shared/ui";

@Component({
  selector: "app-financial-health-widget",
  templateUrl: "./financial-health-widget.html",
  imports: [
    Card,
    CountBadge,
    CurrencyFormatPipe,
    Divider,
    EmptyState,
    Icon,
    RouterLink,
    Skeleton,
    Stack,
    Typography,
  ],
})
export class FinancialHealthWidgetComponent {
  private readonly router = inject(Router);

  public readonly outstandingInvoiceTotal = input<number>(0);
  public readonly paymentsReceivedThisWeek = input<number>(0);
  public readonly overdueInvoiceCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);

  protected readonly hasNoMoneyYet = computed(
    () =>
      this.outstandingInvoiceTotal() === 0 &&
      this.paymentsReceivedThisWeek() === 0 &&
      this.overdueInvoiceCount() === 0,
  );

  /** Invoices are raised off a load, so the first one starts on the load form. */
  protected startFirstInvoice(): void {
    void this.router.navigate(["/loads/add"]);
  }
}
