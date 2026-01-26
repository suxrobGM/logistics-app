import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import type { InvoiceLineItemDto } from "@logistics/shared/api";
import { DividerModule } from "primeng/divider";

@Component({
  selector: "app-payroll-pay-summary",
  templateUrl: "./pay-summary.html",
  imports: [CommonModule, CurrencyPipe, DividerModule],
})
export class PayrollPaySummary {
  public readonly lineItems = input<InvoiceLineItemDto[]>([]);

  protected readonly grossPay = computed(() => {
    return this.lineItems()
      .filter((item) => item.type !== "deduction")
      .reduce((sum, item) => sum + (item.total ?? 0), 0);
  });

  protected readonly totalDeductions = computed(() => {
    return this.lineItems()
      .filter((item) => item.type === "deduction")
      .reduce((sum, item) => sum + (item.total ?? 0), 0);
  });

  protected readonly netPay = computed(() => {
    return this.grossPay() - this.totalDeductions();
  });
}
