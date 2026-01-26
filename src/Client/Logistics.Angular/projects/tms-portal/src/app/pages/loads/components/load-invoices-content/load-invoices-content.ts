import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInvoices } from "@logistics/shared/api";
import type { InvoiceDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { InvoiceStatusTag } from "@/shared/components";

@Component({
  selector: "app-load-invoices-content",
  templateUrl: "./load-invoices-content.html",
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    ProgressSpinnerModule,
    InvoiceStatusTag,
  ],
})
export class LoadInvoicesContent implements OnInit {
  private readonly api = inject(Api);

  readonly loadId = input.required<string>();
  protected readonly isLoading = signal(false);
  protected readonly invoices = signal<InvoiceDto[]>([]);

  ngOnInit(): void {
    this.loadInvoices(this.loadId());
  }

  protected refresh(): void {
    const id = this.loadId();
    if (id) {
      this.loadInvoices(id);
    }
  }

  private async loadInvoices(loadId: string): Promise<void> {
    this.isLoading.set(true);

    const result = await this.api.invoke(getInvoices, {
      LoadId: loadId,
      InvoiceType: "load",
      PageSize: 100,
    });

    if (result?.items) {
      this.invoices.set(result.items);
    }
    this.isLoading.set(false);
  }
}
