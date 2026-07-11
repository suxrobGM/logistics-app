import { DatePipe } from "@angular/common";
import { Component, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe, CurrencyFormatPipe } from "@logistics/shared/pipes";
import { Icon, UiButton, UiDataTable, UiTooltip } from "@logistics/shared/ui";
import { LoadStatusTag, LoadTypeTag } from "@/shared/components";

@Component({
  selector: "app-employee-loads-list",
  templateUrl: "./employee-loads-list.html",
  imports: [
    AddressPipe,
    CurrencyFormatPipe,
    DatePipe,
    Icon,
    LoadStatusTag,
    LoadTypeTag,
    RouterLink,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class EmployeeLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly employeeId = input<string>();
  public readonly isLoading = input(false);
}
