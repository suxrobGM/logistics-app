import { CurrencyPipe } from "@angular/common";
import { Component } from "@angular/core";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";

@Component({
  selector: "cp-invoices-list",
  standalone: true,
  imports: [CurrencyPipe, TableModule, ButtonModule, TagModule],
  templateUrl: "./invoices-list.html",
})
export class InvoicesList {}
