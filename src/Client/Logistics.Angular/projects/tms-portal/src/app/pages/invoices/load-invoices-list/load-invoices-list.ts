import { CommonModule, CurrencyPipe, DatePipe } from "@angular/common";
import { Component, inject, input, type OnInit } from "@angular/core";
import { RouterModule } from "@angular/router";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { DataContainer, InvoiceStatusTag } from "@/shared/components";
import { LoadInvoicesListStore } from "../store/load-invoices-list.store";

@Component({
  selector: "app-load-invoices-list",
  templateUrl: "./load-invoices-list.html",
  providers: [LoadInvoicesListStore],
  imports: [
    CommonModule,
    RouterModule,
    CardModule,
    TableModule,
    CurrencyPipe,
    DatePipe,
    ButtonModule,
    TooltipModule,
    InvoiceStatusTag,
    DataContainer,
  ],
})
export class LoadInvoicesListComponent implements OnInit {
  protected readonly store = inject(LoadInvoicesListStore);
  protected readonly loadId = input<string>();

  ngOnInit(): void {
    const id = this.loadId();
    if (id) {
      this.store.setFilters({ LoadId: id });
    }
  }
}
