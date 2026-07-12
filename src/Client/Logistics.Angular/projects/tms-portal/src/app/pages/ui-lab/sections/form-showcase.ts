import { afterNextRender, Component, computed, input, signal } from "@angular/core";
import {
  disabled,
  form,
  FormField,
  FormRoot,
  required,
  submit,
  validate,
} from "@angular/forms/signals";
import {
  CurrencyField,
  SearchField,
  Typography,
  UiAutocompleteField,
  UiCheckboxField,
  UiDateField,
  UiFormField,
  UiMultiSelectField,
  UiNumberField,
  UiPasswordField,
  UiSelectField,
  UiTextareaField,
  UiTextField,
  UiToggleField,
  ValidatedForm,
  type UiAutocompleteCompleteEvent,
} from "@logistics/shared/ui";

/** The three states every field must be legible in. */
export type LabFormVariant = "pristine" | "invalid" | "disabled";

export interface LabOption {
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

interface LabFormModel {
  text: string;
  textarea: string;
  select: string | null;
  multiselect: string[];
  number: number | null;
  currency: number | null;
  date: Date | null;
  checkbox: boolean;
  toggle: boolean;
  password: string;
  autocomplete: LabOption | null;
}

const EMPTY: LabFormModel = {
  text: "",
  textarea: "",
  select: null,
  multiselect: [],
  number: null,
  currency: null,
  date: null,
  checkbox: false,
  toggle: false,
  password: "",
  autocomplete: null,
};

/**
 * Every `ui-*-field` that exists today, in one `<form [formRoot]>`, rendered once per
 * {@link LabFormVariant}.
 *
 * The model starts empty and every field is `required`, so the *same* form is the pristine case
 * (untouched — no errors visible) and the invalid case (touched via `submit()`, every error
 * visible). `disabled()` in the schema — never a `[disabled]` template binding, which would fight
 * the state input Signal Forms writes — gives the third.
 *
 * This is where wrapper default-drift shows up: a wrapper that quietly changes its own default
 * (`fluid`, `showClear`, `toggleMask`, …) looks identical in a unit test and obviously wrong here.
 */
@Component({
  selector: "app-ui-lab-form-showcase",
  templateUrl: "./form-showcase.html",
  imports: [
    FormRoot,
    FormField,
    ValidatedForm,
    Typography,
    UiFormField,
    UiTextField,
    UiTextareaField,
    UiSelectField,
    UiMultiSelectField,
    UiNumberField,
    CurrencyField,
    UiDateField,
    UiCheckboxField,
    UiToggleField,
    UiPasswordField,
    UiAutocompleteField,
    SearchField,
  ],
})
export class UiLabFormShowcase {
  public readonly variant = input.required<LabFormVariant>();

  protected readonly options = OPTIONS;
  protected readonly suggestions = signal<LabOption[]>([]);
  protected readonly lastSearch = signal("");

  protected readonly model = signal<LabFormModel>({ ...EMPTY });

  private readonly isDisabled = () => this.variant() === "disabled";

  protected readonly form = form(
    this.model,
    (p) => {
      required(p.text, { message: "Text is required." });
      required(p.textarea, { message: "Textarea is required." });
      required(p.select, { message: "Select is required." });
      required(p.number, { message: "Number is required." });
      required(p.currency, { message: "Currency is required." });
      required(p.date, { message: "Date is required." });
      required(p.password, { message: "Password is required." });
      required(p.autocomplete, { message: "Autocomplete is required." });

      // `required()` on an array / boolean is ambiguous (is `[]` empty? is `false`?), so state the
      // rule outright rather than relying on a truthiness that could change under us.
      validate(p.multiselect, ({ value }) =>
        value().length > 0 ? null : { kind: "required", message: "Pick at least one option." },
      );
      validate(p.checkbox, ({ value }) =>
        value() ? null : { kind: "required", message: "Checkbox must be checked." },
      );
      validate(p.toggle, ({ value }) =>
        value() ? null : { kind: "required", message: "Toggle must be on." },
      );

      // Disabled is form *state*, not a template attribute — per field, so nothing depends on
      // whether root-level logic propagates down the tree.
      disabled(p.text, { when: this.isDisabled });
      disabled(p.textarea, { when: this.isDisabled });
      disabled(p.select, { when: this.isDisabled });
      disabled(p.multiselect, { when: this.isDisabled });
      disabled(p.number, { when: this.isDisabled });
      disabled(p.currency, { when: this.isDisabled });
      disabled(p.date, { when: this.isDisabled });
      disabled(p.checkbox, { when: this.isDisabled });
      disabled(p.toggle, { when: this.isDisabled });
      disabled(p.password, { when: this.isDisabled });
      disabled(p.autocomplete, { when: this.isDisabled });
    },
    {
      // The lab makes no HTTP calls. The action exists only so `[formRoot]` owns the submit event
      // (it sets `novalidate` and calls `submit()`), instead of the browser navigating away.
      submission: { action: async () => undefined },
    },
  );

  protected readonly headingId = computed(() => `forms-${this.variant()}`);

  constructor() {
    afterNextRender(() => {
      if (this.variant() !== "invalid") {
        return;
      }
      // Reveal the inline errors the way a real user would, once inputs are set (afterNextRender).
      void submit(this.form, async () => undefined);
    });
  }

  /** Unique per variant — three copies of this form share one document. */
  protected fieldId(name: string): string {
    return `lab-${this.variant()}-${name}`;
  }

  protected search(event: UiAutocompleteCompleteEvent): void {
    const query = event.query.toLowerCase();
    this.suggestions.set(OPTIONS.filter((o) => o.label.toLowerCase().includes(query)));
  }
}
