import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Icon, UiDataTable } from "@logistics/shared/ui";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-customer-loads-list",
  templateUrl: "./customer-loads-list.html",
  imports: [
    AddressPipe,
    ButtonModule,
    CurrencyFormatPipe,
    DatePipe,
    Icon,
    LoadStatusTag,
    LoadTypeTag,
    RouterLink,
    TooltipModule,
    UiDataTable,
  ],
})
export class CustomerLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly customerId = input<string>();
  public readonly isLoading = input(false);
}
