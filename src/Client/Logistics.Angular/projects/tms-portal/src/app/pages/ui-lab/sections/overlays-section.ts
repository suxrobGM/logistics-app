import { Component, computed, signal } from "@angular/core";
import {
  Typography,
  UiAutocompleteField,
  UiDateField,
  UiMultiSelectField,
  UiSelectField,
  type UiAutocompleteCompleteEvent,
} from "@logistics/shared/ui";

interface LabOption {
  readonly label: string;
  readonly value: string;
}

/** Mutable on purpose: `ui-multiselect-field` declares `options` as `unknown[]`, not `readonly`. */
const OPTIONS: LabOption[] = [
  { label: "Dry Van", value: "dry-van" },
  { label: "Reefer", value: "reefer" },
  { label: "Flatbed", value: "flatbed" },
  { label: "Step Deck", value: "step-deck" },
  { label: "Tanker", value: "tanker" },
];

/**
 * Every control that opens a CDK overlay, unbound from any form, one per card.
 *
 * These are the ones that break in ways nothing else catches: a dropped `<ng-template>` or a
 * missing `*hlmSelectPortal` compiles, passes every unit test, renders a perfectly normal-looking
 * trigger — and then the panel never closes. The only way to know is to click it, so each card is
 * a click target with a stable id and its live value printed underneath.
 */
@Component({
  selector: "app-ui-lab-overlays",
  templateUrl: "./overlays-section.html",
  imports: [Typography, UiSelectField, UiMultiSelectField, UiAutocompleteField, UiDateField],
})
export class UiLabOverlaysSection {
  protected readonly options = OPTIONS;
  protected readonly suggestions = signal<LabOption[]>([]);

  protected readonly selectValue = signal<unknown>(null);
  protected readonly multiselectValue = signal<unknown[]>([]);
  protected readonly autocompleteValue = signal<LabOption | null>(null);
  protected readonly dateValue = signal<Date | null>(null);

  protected readonly selectJson = computed(() => JSON.stringify(this.selectValue()));
  protected readonly multiselectJson = computed(() => JSON.stringify(this.multiselectValue()));
  protected readonly autocompleteJson = computed(() => JSON.stringify(this.autocompleteValue()));
  protected readonly dateJson = computed(() => this.dateValue()?.toISOString() ?? "null");

  protected search(event: UiAutocompleteCompleteEvent): void {
    const query = event.query.toLowerCase();
    this.suggestions.set(OPTIONS.filter((o) => o.label.toLowerCase().includes(query)));
  }
}
