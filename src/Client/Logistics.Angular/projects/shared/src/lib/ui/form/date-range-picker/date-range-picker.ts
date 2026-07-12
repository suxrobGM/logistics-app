import { booleanAttribute, Component, computed, input, output, viewChild } from "@angular/core";
import { provideNativeDateAdapter } from "@spartan-ng/brain/date-time";
import { DateUtils, PredefinedDateRanges } from "../../../utils";
import { UiButton } from "../../action/button/button";
import { HlmDatePickerImports, HlmDateRangePicker } from "../../primitives/date-picker";

export interface DatePreset {
  label: string;
  getRange: () => { startDate: Date; endDate: Date };
}

/** Default preset options for common date ranges. */
export const DEFAULT_DATE_PRESETS: DatePreset[] = [
  { label: "This Week", getRange: () => PredefinedDateRanges.getThisWeek() },
  { label: "Last Week", getRange: () => PredefinedDateRanges.getLastWeek() },
  { label: "This Month", getRange: () => PredefinedDateRanges.getThisMonth() },
  { label: "Last Month", getRange: () => PredefinedDateRanges.getLastMonth() },
];

const pad = (value: number): string => String(value).padStart(2, "0");

/** Formats one end of the range as `mm/dd/yyyy`. */
const formatOne = (date: Date | null): string =>
  date ? `${pad(date.getMonth() + 1)}/${pad(date.getDate())}/${date.getFullYear()}` : "";

/**
 * Date range picker with preset buttons in the calendar footer. The app's only range-capable date
 * control; `ui-date-field` is deliberately single-date.
 *
 * `datesChange` only fires once both ends are picked — call sites push it straight into a filter and
 * refetch, so a half-picked range must stay silent.
 *
 * @example
 * <ui-date-range-picker [dates]="dateRange()" (datesChange)="onDateChange($event)" />
 */
@Component({
  selector: "ui-date-range-picker",
  templateUrl: "./date-range-picker.html",
  imports: [HlmDatePickerImports, UiButton],
  providers: [provideNativeDateAdapter()],
})
export class DateRangePicker {
  public readonly presets = input<DatePreset[]>(DEFAULT_DATE_PRESETS);
  public readonly dates = input<Date[]>([]);
  public readonly datesChange = output<Date[]>();

  public readonly placeholder = input<string>("Select date range");
  public readonly inputId = input<string>("");
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly minDate = input<Date | undefined>(undefined);
  public readonly maxDate = input<Date | undefined>(undefined);

  /**
   * Defaulted off, unlike brain's `BrnDateInput.showClear`, which defaults to true: its clear emits
   * `null`, which no call site's `onDateRangeChange(dates: Date[])` expects. Opting in emits `[]`.
   */
  public readonly showClear = input(false, { transform: booleanAttribute });

  private readonly picker = viewChild.required<HlmDateRangePicker<Date>>(HlmDateRangePicker);

  /** The public API is a `Date[]`; the Helm picker wants a `[start, end]` tuple. */
  protected readonly range = computed<[Date, Date] | undefined>(() => {
    const dates = this.dates();
    return DateUtils.isValidRange(dates) ? [dates[0], dates[1]] : undefined;
  });

  protected readonly formatDates = (dates: [Date | null, Date | null]): string => {
    const [start, end] = dates;
    if (!start && !end) return "";
    return `${formatOne(start)} - ${formatOne(end)}`;
  };

  protected onRangeChange(range: [Date, Date] | null): void {
    if (!range) {
      // Only reachable via the opt-in clear button.
      this.datesChange.emit([]);
      return;
    }
    const dates = [range[0], range[1]];
    if (DateUtils.isValidRange(dates)) {
      this.datesChange.emit(dates);
    }
  }

  protected selectPreset(preset: DatePreset): void {
    const { startDate, endDate } = preset.getRange();
    // `updateDate` writes the picker's internal model and emits `dateChange`, so a preset notifies
    // the parent through the same path as a manual pick.
    this.picker().updateDate([startDate, endDate]);
  }
}
