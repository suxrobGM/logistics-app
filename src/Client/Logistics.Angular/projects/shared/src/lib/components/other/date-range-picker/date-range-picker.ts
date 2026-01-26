import { Component, computed, effect, input, model, output, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { ButtonModule } from "primeng/button";
import { DatePicker } from "primeng/datepicker";
import { DateUtils, PredefinedDateRanges } from "../../../utils";

export interface DatePreset {
  label: string;
  value: string;
  getRange: () => { startDate: Date; endDate: Date };
}

/** Default preset options for common date ranges */
export const DEFAULT_DATE_PRESETS: DatePreset[] = [
  {
    label: "This Week",
    value: "this_week",
    getRange: () => PredefinedDateRanges.getThisWeek(),
  },
  {
    label: "Last Week",
    value: "last_week",
    getRange: () => PredefinedDateRanges.getLastWeek(),
  },
  {
    label: "This Month",
    value: "this_month",
    getRange: () => PredefinedDateRanges.getThisMonth(),
  },
  {
    label: "Last Month",
    value: "last_month",
    getRange: () => PredefinedDateRanges.getLastMonth(),
  },
];

/**
 * Flexible date range calendar component.
 * Supports predefined date presets and optional apply button.
 *
 * @example
 * // Basic usage with presets
 * <ui-date-range-picker [showPresets]="true" (dateRangeChange)="onDateChange($event)" />
 *
 * // With apply button
 * <ui-date-range-picker [showApplyButton]="true" (applyButtonClick)="onApply()" />
 *
 * // Two-way binding
 * <ui-date-range-picker [(startDate)]="start" [(endDate)]="end" />
 */
@Component({
  selector: "ui-date-range-picker",
  templateUrl: "./date-range-picker.html",
  imports: [DatePicker, FormsModule, ButtonModule],
})
export class DateRangePicker {
  protected readonly todayDate = DateUtils.today();
  protected readonly dates = signal<Date[]>([]);

  // Display options
  public readonly label = input<string | null>(null);
  public readonly showPresets = input(false);
  public readonly showApplyButton = input(false);
  public readonly presets = input<DatePreset[]>(DEFAULT_DATE_PRESETS);
  public readonly initialPreset = input<string | null>(null);

  // Two-way binding for dates
  public readonly startDate = model<Date | null>(null);
  public readonly endDate = model<Date | null>(null);

  // Events
  public readonly startDateChange = output<Date | null>();
  public readonly endDateChange = output<Date | null>();
  public readonly dateRangeChange = output<Date[]>();
  public readonly applyButtonClick = output<void>();

  // Internal state
  protected readonly selectedPreset = signal<string | null>(null);

  protected readonly availablePresets = computed(() => {
    const presets = this.presets();
    // Add "Custom" option if not already present
    if (!presets.some((p) => p.value === "custom")) {
      return [
        ...presets,
        {
          label: "Custom",
          value: "custom",
          getRange: () => PredefinedDateRanges.getLastWeek(),
        },
      ];
    }
    return presets;
  });

  constructor() {
    // Sync dates signal with model inputs
    effect(() => {
      if (this.startDate() && this.endDate()) {
        this.dates.set([this.startDate()!, this.endDate()!]);
      }
    });

    // Initialize with preset if provided
    const initial = this.initialPreset();
    if (initial) {
      this.selectPresetByValue(initial);
    }
  }

  areDatesValid(): boolean {
    return DateUtils.isValidRange(this.dates());
  }

  selectPreset(preset: DatePreset): void {
    this.selectedPreset.set(preset.value);
    if (preset.value !== "custom") {
      const range = preset.getRange();
      this.dates.set([range.startDate, range.endDate]);
      this.updateModels();
      this.emitDateRangeChange();
    }
  }

  onDatePickerChange(): void {
    this.selectedPreset.set("custom");
    this.updateModels();
    if (this.areDatesValid()) {
      this.emitDateRangeChange();
    }
  }

  onClickApplyButton(): void {
    this.updateModels();
    this.applyButtonClick.emit();
  }

  /** Programmatically set the date range */
  setDateRange(dates: Date[]): void {
    this.dates.set(dates);
    this.selectedPreset.set("custom");
  }

  private selectPresetByValue(value: string): void {
    const preset = this.availablePresets().find((p) => p.value === value);
    if (preset && preset.value !== "custom") {
      this.selectedPreset.set(value);
      const range = preset.getRange();
      this.dates.set([range.startDate, range.endDate]);
      this.updateModels();
    }
  }

  private updateModels(): void {
    const currentDates = this.dates();
    if (currentDates.length === 2) {
      this.startDate.set(currentDates[0]);
      this.endDate.set(currentDates[1]);
      this.startDateChange.emit(currentDates[0]);
      this.endDateChange.emit(currentDates[1]);
    }
  }

  private emitDateRangeChange(): void {
    const currentDates = this.dates();
    if (currentDates.length === 2) {
      this.dateRangeChange.emit(currentDates);
    }
  }
}
