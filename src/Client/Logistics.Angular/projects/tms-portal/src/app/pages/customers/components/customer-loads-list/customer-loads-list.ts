import { CurrencyPipe, DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-customer-loads-list",
  templateUrl: "./customer-loads-list.html",
  imports: [
    TableModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyPipe,
    DatePipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
  ],
})
export class CustomerLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly customerId = input<string>();
  public readonly isLoading = input(false);
}
