import { CurrencyPipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-truck-loads-list",
  templateUrl: "./truck-loads-list.html",
  imports: [
    TableModule,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyPipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
  ],
})
export class TruckLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly isLoading = input(false);
}
