import { DecimalPipe } from "@angular/common";
import { Component, input, output } from "@angular/core";
import { type LoadBoardListingDto } from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { Badge, UiButton, UiDataTable, UiTooltip } from "@logistics/shared/ui";
import { getProviderSeverity } from "../loadboard.constants";

@Component({
  selector: "app-loadboard-search-results",
  templateUrl: "./loadboard-search-results.html",
  imports: [
    Badge,
    CurrencyFormatPipe,
    DateFormatPipe,
    DecimalPipe,
    DistanceUnitPipe,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class LoadBoardSearchResults {
  public readonly listings = input.required<LoadBoardListingDto[]>();
  public readonly book = output<LoadBoardListingDto>();

  protected readonly getProviderSeverity = getProviderSeverity;
}
