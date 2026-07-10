import { Component, effect, input, linkedSignal, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { DateUtils, PredefinedDateRanges } from "../../../utils";

export interface DatePreset {
  label: string;
  getRange: () => { startDate: Date; endDate: Date };
}

/** Default preset options for common date ranges */
export const DEFAULT_DATE_PRESETS: DatePreset[] = [
  { label: "This Week", getRange: () => PredefinedDateRanges.getThisWeek() },
  { label: "Last Week", getRange: () => PredefinedDateRanges.getLastWeek() },
  { label: "This Month", getRange: () => PredefinedDateRanges.getThisMonth() },
  { label: "Last Month", getRange: () => PredefinedDateRanges.getLastMonth() },
];

/**
 * Date range picker with preset buttons in the calendar footer.
 *
 * `datesChange` only fires once both ends of the range are picked.
 *
 * @example
 * <ui-date-range-picker [dates]="dateRange()" (datesChange)="onDateChange($event)" />
 */
@Component({
  selector: "ui-date-range-picker",
  templateUrl: "./date-range-picker.html",
  imports: [DatePicker, FormsModule, ButtonModule],
})
export class DateRangePicker {
  public readonly presets = input<DatePreset[]>(DEFAULT_DATE_PRESETS);
  public readonly dates = input<Date[]>([]);
  public readonly datesChange = output<Date[]>();

  /** Working value the datepicker writes into; seeded from `dates`. */
  protected readonly selected = linkedSignal<Date[]>(() => this.dates());

  constructor() {
    effect(() => {
      const dates = this.selected();
      if (DateUtils.isValidRange(dates)) {
        this.datesChange.emit(dates);
      }
    });
  }

  selectPreset(preset: DatePreset): void {
    const range = preset.getRange();
    this.selected.set([range.startDate, range.endDate]);
  }
}
