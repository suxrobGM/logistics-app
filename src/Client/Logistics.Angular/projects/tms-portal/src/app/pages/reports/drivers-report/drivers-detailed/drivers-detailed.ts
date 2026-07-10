import { DecimalPipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { LocalizationService } from "@logistics/shared";
import { Icon, Stack, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { InputTextModule } from "primeng/inputtext";
import { TableModule } from "primeng/table";
import { Tag, TagModule } from "primeng/tag";
import { DateRangePicker, PageHeader } from "@/shared/components";
import { DateUtils, getPerformanceLevel, getPerformanceSeverity } from "@/shared/utils";
import { DriversDetailedStore } from "./store";

@Component({
  selector: "app-drivers-detailed",
  templateUrl: "./drivers-detailed.html",
  providers: [DriversDetailedStore],
  imports: [
    FormsModule,
    ButtonModule,
    CardModule,
    InputTextModule,
    TableModule,
    DatePickerModule,
    DateRangePicker,
    CurrencyFormatPipe,
    DecimalPipe,
    TagModule,
    PageHeader,
    Icon,
    Stack,
    Typography,
  ],
})
export class DriversDetailedComponent implements OnInit {
  private readonly localization = inject(LocalizationService);
  protected readonly store = inject(DriversDetailedStore);
  protected readonly distanceUnitLabel = this.localization.getDistanceUnitLabel();

  protected readonly startDate = signal(DateUtils.thisYear());
  protected readonly endDate = signal(DateUtils.today());

  ngOnInit(): void {
    // Seed the date range without loading: the table issues its own initial
    // lazy-load, which then carries StartDate/EndDate (matching the old query()).
    this.store.setFilters(
      {
        StartDate: this.startDate().toISOString(),
        EndDate: this.endDate().toISOString(),
      },
      { reload: false },
    );
  }

  protected onSearch(event: Event): void {
    const value = (event.target as HTMLInputElement).value;
    this.store.setSearch(value);
  }

  protected onDateRangeChange(dates: Date[]): void {
    if (dates.length === 2) {
      this.startDate.set(dates[0]);
      this.endDate.set(dates[1]);
      this.store.setFilters({
        StartDate: this.startDate().toISOString(),
        EndDate: this.endDate().toISOString(),
      });
    }
  }

  protected getPerformanceLevel = getPerformanceLevel;
  protected getPerformanceSeverity = getPerformanceSeverity;

  protected getDriverTypeSeverity(isMainDriver: boolean): Tag["severity"] {
    return isMainDriver ? "success" : "info";
  }
}
