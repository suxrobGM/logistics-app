import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
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
  public readonly outstandingInvoiceTotal = input<number>(0);
  public readonly paymentsReceivedThisWeek = input<number>(0);
  public readonly overdueInvoiceCount = input<number>(0);
  public readonly isLoading = input<boolean>(false);

  protected readonly hasNothingOwing = computed(
    () =>
      this.outstandingInvoiceTotal() === 0 &&
      this.paymentsReceivedThisWeek() === 0 &&
      this.overdueInvoiceCount() === 0,
  );
}
