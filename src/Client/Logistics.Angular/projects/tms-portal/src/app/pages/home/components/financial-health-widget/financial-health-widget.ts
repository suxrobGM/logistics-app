import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import { CurrencyFormatPipe } from "@logistics/shared";
import { Card, CountBadge, Divider, Icon, Skeleton, Stack, Typography } from "@logistics/shared/ui";

@Component({
  selector: "app-financial-health-widget",
  templateUrl: "./financial-health-widget.html",
  imports: [
    Card,
    CountBadge,
    CurrencyFormatPipe,
    Divider,
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
}
