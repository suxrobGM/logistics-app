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
  selector: "app-employee-loads-list",
  templateUrl: "./employee-loads-list.html",
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
export class EmployeeLoadsList {
  public readonly loads = input<LoadDto[]>([]);
  public readonly employeeId = input<string>();
  public readonly isLoading = input(false);
}
