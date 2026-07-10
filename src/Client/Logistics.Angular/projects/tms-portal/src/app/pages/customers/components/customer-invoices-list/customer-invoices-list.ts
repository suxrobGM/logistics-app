import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { InvoiceDto } from "@logistics/shared/api";
import { UiDataTable } from "@logistics/shared/components";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-customer-invoices-list",
  templateUrl: "./customer-invoices-list.html",
  imports: [
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyFormatPipe,
    DatePipe,
    InvoiceStatusTag,
    UiDataTable,
  ],
})
export class CustomerInvoicesList {
  public readonly invoices = input<InvoiceDto[]>([]);
  public readonly customerId = input<string>();
  public readonly isLoading = input(false);
}
