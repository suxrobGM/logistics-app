import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { InvoiceDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-customer-invoices-list",
  templateUrl: "./customer-invoices-list.html",
  imports: [
    TableModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    InvoiceStatusTag,
  ],
})
export class CustomerInvoicesList {
  public readonly invoices = input<InvoiceDto[]>([]);
  public readonly customerId = input<string>();
  public readonly isLoading = input(false);
}
