import { Component, computed, effect, inject, input, linkedSignal, output } from "@angular/core";
import { form, FormField, FormRoot, max, min, required } from "@angular/forms/signals";
import { RouterLink } from "@angular/router";
import { CA_PROVINCES_OPTIONS, ToastService, US_STATES_OPTIONS } from "@logistics/shared";
import {
  UiButton,
  UiFormField,
  UiNumberField,
  UiSelectField,
  ValidatedForm,
} from "@logistics/shared/ui";

export interface IftaRateFormValue {
  countryCode: "US" | "CA";
  /** Empty string means a country-wide rate (no region). */
  region: string;
  year: number;
  quarter: number;
  ratePerGallon: number | null;
  surchargePerGallon: number | null;
}

const COUNTRY_OPTIONS = [
  { label: "United States", value: "US" },
  { label: "Canada", value: "CA" },
];

const QUARTER_OPTIONS = [1, 2, 3, 4].map((q) => ({ label: `Q${q}`, value: q }));

const COUNTRY_WIDE_OPTION = { label: "Country-wide", value: "" };

const EMPTY: IftaRateFormValue = {
  countryCode: "US",
  region: "",
  year: new Date().getFullYear(),
  quarter: Math.floor(new Date().getMonth() / 3) + 1,
  ratePerGallon: null,
  surchargePerGallon: null,
};

@Component({
  selector: "adm-ifta-rate-form",
  templateUrl: "./ifta-rate-form.html",
  imports: [
    FormField,
    FormRoot,
    RouterLink,
    UiButton,
    UiFormField,
    UiNumberField,
    UiSelectField,
    ValidatedForm,
  ],
})
export class IftaRateForm {
  private readonly toastService = inject(ToastService);

  public readonly mode = input.required<"create" | "edit">();
  public readonly initial = input<Partial<IftaRateFormValue> | null>(null);
  public readonly isLoading = input(false);

  public readonly save = output<IftaRateFormValue>();
  public readonly remove = output<void>();

  protected readonly countryOptions = COUNTRY_OPTIONS;
  protected readonly quarterOptions = QUARTER_OPTIONS;

  /** Seeded from `initial()`; resets to those values whenever the input changes. */
  protected readonly model = linkedSignal<IftaRateFormValue>(() => ({
    ...EMPTY,
    ...(this.initial() ?? {}),
  }));

  protected readonly regionOptions = computed(() => {
    const states = this.model().countryCode === "CA" ? CA_PROVINCES_OPTIONS : US_STATES_OPTIONS;
    return [COUNTRY_WIDE_OPTION, ...states];
  });

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.countryCode, { message: "Country is required." });
      required(p.year, { message: "Year is required." });
      min(p.year, 2000, { message: "Year must be 2000 or later." });
      max(p.year, 2100, { message: "Year cannot be after 2100." });
      required(p.quarter, { message: "Quarter is required." });
      required(p.ratePerGallon, { message: "Rate per gallon is required." });
      min(p.ratePerGallon, 0, { message: "Rate per gallon cannot be negative." });
      min(p.surchargePerGallon, 0, { message: "Surcharge cannot be negative." });
    },
    {
      submission: {
        action: async () => {
          this.save.emit(this.model());
          return undefined;
        },
      },
    },
  );

  constructor() {
    // Switching country invalidates a selected state/province, so fall back to country-wide
    effect(() => {
      const country = this.model().countryCode;
      const region = this.model().region;
      const states = country === "CA" ? CA_PROVINCES_OPTIONS : US_STATES_OPTIONS;
      if (region && !states.some((opt) => opt.value === region)) {
        this.model.update((v) => ({ ...v, region: "" }));
      }
    });
  }

  protected askRemove(): void {
    this.toastService.confirm({
      message:
        "Are you sure that you want to delete this IFTA tax rate? This action cannot be undone.",
      header: "Confirm Delete",
      icon: "warning",
      severity: "danger",
      accept: () => this.remove.emit(),
    });
  }
}
