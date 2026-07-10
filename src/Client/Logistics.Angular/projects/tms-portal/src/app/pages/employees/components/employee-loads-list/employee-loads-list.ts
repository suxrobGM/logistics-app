import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { UiDataTable } from "@logistics/shared/components";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { TooltipModule } from "primeng/tooltip";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-employee-loads-list",
  templateUrl: "./employee-loads-list.html",
  imports: [
    UiDataTable,
    ButtonModule,
    TooltipModule,
    RouterLink,
    CurrencyFormatPipe,
    DatePipe,
    AddressPipe,
    LoadStatusTag,
    LoadTypeTag,
  ],
})
export class EmployeeLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly employeeId = input<string>();
  public readonly isLoading = input(false);
}
