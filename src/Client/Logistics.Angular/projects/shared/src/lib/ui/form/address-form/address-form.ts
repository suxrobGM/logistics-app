import {
  Component,
  computed,
  ElementRef,
  inject,
  input,
  linkedSignal,
  model,
  output,
} from "@angular/core";
import type { FormValueControl } from "@angular/forms/signals";
import type { Address } from "../../../api/generated/models";
import {
  AU_STATES_OPTIONS,
  CA_PROVINCES_OPTIONS,
  COUNTRIES_OPTIONS,
  DEFAULT_COUNTRY_OPTION,
  DEFAULT_STATE_FIELD_LABEL,
  STATE_FIELD_LABELS,
  US_STATES_OPTIONS,
} from "../../../constants";
import type { SelectOption } from "../../../models/select-option";
import { findOption } from "../../../utils/select-utils";
import { HlmInput } from "../../primitives/input";
import { focusFirstControl } from "../focus-control";
import { UiFormField } from "../form-field/form-field";
import { UiSelectField } from "../select-field/select-field";

/** Country-specific state/province option lists. Countries not listed get a free-text input. */
const COUNTRY_STATE_OPTIONS: Record<string, SelectOption[]> = {
  US: US_STATES_OPTIONS,
  CA: CA_PROVINCES_OPTIONS,
  AU: AU_STATES_OPTIONS,
};

/** Internal, editable representation of the composite address value. */
interface AddressParts {
  addressLine1: string;
  addressLine2: string | null;
  city: string;
  state: string;
  zipCode: string;
  country: string;
}

/**
 * Splits an incoming {@link Address} into editable parts: the country is normalised to an ISO code,
 * and the state to a matching option value when the country has a fixed option list.
 */
function parseAddress(value: Address | null): AddressParts {
  if (!value) {
    return {
      addressLine1: "",
      addressLine2: null,
      city: "",
      state: "",
      zipCode: "",
      country: DEFAULT_COUNTRY_OPTION.value,
    };
  }

  const countryOption = findOption(COUNTRIES_OPTIONS, value.country ?? "");
  const countryCode = countryOption?.value ?? DEFAULT_COUNTRY_OPTION.value;
  const stateOptions = COUNTRY_STATE_OPTIONS[countryCode];
  const stateValue = stateOptions
    ? (findOption(stateOptions, value.state ?? "")?.value ?? "")
    : (value.state ?? "");

  return {
    addressLine1: value.line1 ?? "",
    addressLine2: value.line2 ?? null,
    city: value.city ?? "",
    state: stateValue,
    zipCode: value.zipCode ?? "",
    country: countryCode,
  };
}

/**
 * Composite address editor.
 *
 * Implements `FormValueControl<Address | null>` only, never a legacy `ControlValueAccessor` — see
 * `text-field.ts` for the bridge contract. The sub-fields are plain signals, not a nested form: each
 * edit recomposes an `Address` and pushes it through `value`, but only once every required part is
 * present, so an incomplete address is never emitted.
 */
@Component({
  selector: "ui-address-form",
  templateUrl: "./address-form.html",
  imports: [UiFormField, UiSelectField, HlmInput],
})
export class AddressForm implements FormValueControl<Address | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<Address | null>(null);

  /** Driven by the forms bridge; disables every sub-field when true. */
  public readonly disabled = input<boolean>(false);

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  /**
   * Optional list of ISO-3166-1 alpha-2 country codes to allow.
   * When omitted, all countries are shown.
   */
  public readonly allowedCountries = input<readonly string[] | null>(null);

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  /**
   * Editable parts, re-derived whenever the bound value changes externally. User edits update the
   * parts in place (see the `on*` handlers) and re-compose a new value; there is no write-back loop
   * because `value.set` is only ever called from a user-input handler, never reactively.
   */
  protected readonly parts = linkedSignal<Address | null, AddressParts>({
    source: this.value,
    computation: (value) => parseAddress(value),
  });

  protected readonly countries = computed(() => {
    const allowed = this.allowedCountries();
    if (!allowed || allowed.length === 0) {
      return COUNTRIES_OPTIONS;
    }
    const set = new Set(allowed);
    return COUNTRIES_OPTIONS.filter((opt) => set.has(opt.value));
  });

  /** Currently selected country code. */
  private readonly country = computed(() => this.parts().country);

  /** State/province options for the selected country, or null when free-text. */
  protected readonly stateOptions = computed<SelectOption[] | null>(
    () => COUNTRY_STATE_OPTIONS[this.country()] ?? null,
  );

  protected readonly hasStateOptions = computed(() => this.stateOptions() !== null);

  /** Country-aware label for the state/region/province field. */
  protected readonly stateLabel = computed(
    () => STATE_FIELD_LABELS[this.country()] ?? DEFAULT_STATE_FIELD_LABEL,
  );

  protected onLine1Input(event: Event): void {
    this.updatePart("addressLine1", (event.target as HTMLInputElement).value);
  }

  protected onLine2Input(event: Event): void {
    this.updatePart("addressLine2", (event.target as HTMLInputElement).value);
  }

  protected onCityInput(event: Event): void {
    this.updatePart("city", (event.target as HTMLInputElement).value);
  }

  protected onStateInput(event: Event): void {
    this.updatePart("state", (event.target as HTMLInputElement).value);
  }

  protected onStateSelect(value: string): void {
    this.updatePart("state", value ?? "");
  }

  protected onZipCodeInput(event: Event): void {
    this.updatePart("zipCode", (event.target as HTMLInputElement).value);
  }

  /**
   * A country change also clears the state when the input mode flips between a fixed option list and
   * free text — a stale value is meaningless in the other mode.
   */
  protected onCountrySelect(newCountry: string): void {
    this.parts.update((parts) => {
      const prevHadOptions = !!COUNTRY_STATE_OPTIONS[parts.country];
      const newHasOptions = !!COUNTRY_STATE_OPTIONS[newCountry];
      return {
        ...parts,
        country: newCountry,
        state: prevHadOptions !== newHasOptions ? "" : parts.state,
      };
    });
    this.emit();
  }

  private updatePart<K extends keyof AddressParts>(key: K, value: AddressParts[K]): void {
    this.parts.update((parts) => ({ ...parts, [key]: value }));
    this.emit();
  }

  /**
   * Recomposes an `Address` from the current parts and pushes it through `value`. Skips the write
   * while any required part is missing, so the bound control keeps its previous value and stays
   * invalid via the parent field's `required` rule.
   */
  private emit(): void {
    const parts = this.parts();

    if (!parts.addressLine1 || !parts.city || !parts.state || !parts.zipCode || !parts.country) {
      return;
    }

    const countryOption = findOption(COUNTRIES_OPTIONS, parts.country);

    const address: Address = {
      line1: parts.addressLine1,
      line2: parts.addressLine2,
      city: parts.city,
      state: parts.state,
      zipCode: parts.zipCode,
      // The ISO-3166-1 alpha-2 code, not the display label: the backend AddressValidator requires a
      // 2-letter country code.
      country: countryOption?.value ?? DEFAULT_COUNTRY_OPTION.value,
    };

    this.value.set(address);
  }
}
