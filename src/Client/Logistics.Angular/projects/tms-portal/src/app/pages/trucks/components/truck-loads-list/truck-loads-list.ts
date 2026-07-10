import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import { UiDataTable } from "@logistics/shared/ui";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-truck-loads-list",
  templateUrl: "./truck-loads-list.html",
  imports: [
    UiDataTable,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyFormatPipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
  ],
})
export class TruckLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly isLoading = input(false);
}
