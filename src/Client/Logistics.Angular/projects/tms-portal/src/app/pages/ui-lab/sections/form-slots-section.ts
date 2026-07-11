import { DatePipe } from "@angular/common";
import { Component, signal } from "@angular/core";
import {
  DateRangePicker,
  Typography,
  UiAutocompleteField,
  UiButton,
  UiFormField,
  UiSelectField,
  type UiAutocompleteCompleteEvent,
} from "@logistics/shared/ui";

interface Provider {
  label: string;
  value: string;
  description: string;
}

interface Person {
  id: string;
  fullName: string;
  email: string;
}

const PROVIDERS: Provider[] = [
  { label: "Demo", value: "demo", description: "Any text works as an API key — testing only." },
  { label: "Samsara", value: "samsara", description: "Settings → API Tokens in the dashboard." },
  { label: "Motive", value: "motive", description: "Settings → Integrations → API Keys." },
  { label: "Geotab", value: "geotab", description: "API key as database|userName." },
];

const PEOPLE: Person[] = [
  { id: "1", fullName: "Ada Lovelace", email: "ada@example.com" },
  { id: "2", fullName: "Grace Hopper", email: "grace@example.com" },
  { id: "3", fullName: "Alan Turing", email: "alan@example.com" },
];

/**
 * The capabilities S10a added to the form wrappers, isolated so a regression is obvious:
 *
 * - `ui-select-field` `#item` (per-option renderer) and `#selectedItem` (trigger renderer). These
 *   are *distinct* slots — the ELD selects render an icon + one-liner in the trigger and a
 *   two-line detail in the panel, and a previous agent silently dropped the sub-line.
 * - `ui-autocomplete-field` `#item` and `#empty` (the latter carries a "create new" action at two
 *   real call sites, so it is not decorative).
 * - `ui-date-range-picker` — the app's only range-capable date control, now on the vendored
 *   `hlm-date-range-picker`. It emits **only** when both ends are picked.
 *
 * Each slot is also shown *unset* directly above its set version: a slot that leaks into the
 * default path (an unconditional `#item` forcing custom rendering for every option) shows up here
 * as the plain select suddenly rendering rich rows.
 */
@Component({
  selector: "app-ui-lab-form-slots",
  templateUrl: "./form-slots-section.html",
  imports: [
    DatePipe,
    DateRangePicker,
    Typography,
    UiAutocompleteField,
    UiButton,
    UiFormField,
    UiSelectField,
  ],
})
export class UiLabFormSlotsSection {
  protected readonly providers = PROVIDERS;

  protected readonly plainProvider = signal<string | null>(null);
  protected readonly slottedProvider = signal<string | null>(null);
  protected readonly selectedPerson = signal<Person | null>(null);

  protected readonly suggestions = signal<Person[]>([]);
  protected readonly lastQuery = signal("");

  /** Proves `datesChange` fires only on a complete range. */
  protected readonly range = signal<Date[]>([]);
  protected readonly rangeEmissions = signal(0);

  protected search(event: UiAutocompleteCompleteEvent): void {
    const query = event.query.toLowerCase();
    this.lastQuery.set(event.query);
    this.suggestions.set(PEOPLE.filter((p) => p.fullName.toLowerCase().includes(query)));
  }

  protected onRange(dates: Date[]): void {
    this.range.set(dates);
    this.rangeEmissions.update((n) => n + 1);
  }
}
