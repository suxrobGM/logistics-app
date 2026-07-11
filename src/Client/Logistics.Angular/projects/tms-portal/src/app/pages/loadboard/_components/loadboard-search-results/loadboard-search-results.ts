import { DecimalPipe } from "@angular/common";
import { Component, input, output } from "@angular/core";
import { type LoadBoardListingDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { UiButton, UiDataTable } from "@logistics/shared/ui";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { getProviderSeverity } from "../loadboard.constants";

@Component({
  selector: "app-loadboard-search-results",
  templateUrl: "./loadboard-search-results.html",
  imports: [
    CurrencyFormatPipe,
    DateFormatPipe,
    DecimalPipe,
    DistanceUnitPipe,
    TagModule,
    TooltipModule,
    UiButton,
    UiDataTable,
  ],
})
export class LoadBoardSearchResults {
  public readonly listings = input.required<LoadBoardListingDto[]>();
  public readonly book = output<LoadBoardListingDto>();

  protected readonly getProviderSeverity = getProviderSeverity;
}
