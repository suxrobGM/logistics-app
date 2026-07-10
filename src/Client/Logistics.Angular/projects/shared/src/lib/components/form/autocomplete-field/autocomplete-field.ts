import { booleanAttribute, Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { AutoComplete, type AutoCompleteCompleteEvent } from "primeng/autocomplete";

/**
 * Type-ahead autocomplete backed by PrimeNG's `p-autocomplete`.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * `p-autocomplete` extends `BaseInput` (a `ControlValueAccessor`) and so must never carry
 * `formControlName` / `[formField]` itself: every `BaseInput` subclass collides with Signal
 * Forms' `pattern` state input under strictTemplates (`pattern: string` vs the Signal Forms
 * `pattern` control). Instead we drive it with a standalone `NgModel`
 * (`[ngModel]` / `(ngModelChange)` + `{ standalone: true }`); the inner NgModel lives in THIS
 * view, not in `ui-form-field`'s projected content, so `ui-form-field`'s `contentChild(NgControl)`
 * still resolves the OUTER binding. See `forms/signal-forms-compat-probe.spec.ts`.
 *
 * The parent still owns fetching suggestions: wire `(completeMethod)` to your search handler and
 * feed the result back through `[suggestions]`.
 *
 * @example
 * <ui-form-field label="Driver" for="driver" [required]="true">
 *   <ui-autocomplete-field id="driver" formControlName="driver"
 *     [suggestions]="suggestedDrivers()" optionLabel="fullName" [minQueryLength]="2"
 *     (completeMethod)="searchDriver($event)" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-autocomplete-field",
  templateUrl: "./autocomplete-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [AutoComplete, FormsModule],
})
export class UiAutocompleteField<T = unknown> implements FormValueControl<T | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T | null>(null);

  // Optional state inputs. Signal Forms binds these automatically when present;
  // the Reactive Forms bridge drives `disabled`.
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  /**
   * Raised when p-autocomplete needs suggestions. Mirrors `completeMethod`; carries `{ query }`.
   * Every call site uses this to fetch and set `suggestions`.
   */
  public readonly completeMethod = output<AutoCompleteCompleteEvent>();

  /** Raised after the user picks a suggestion. Mirrors p-autocomplete's `onSelect`; emits the chosen value. */
  public readonly optionSelected = output<T | null>();

  /** Raised when the user clicks the clear (x) button. Mirrors p-autocomplete's `onClear`. */
  public readonly cleared = output<void>();

  // Presentation. Every default below mirrors the PrimeNG AutoComplete class default
  // (see node_modules/primeng/fesm2022/primeng-autocomplete.mjs) — do not "improve" them.
  public readonly suggestions = input.required<readonly T[]>();
  /** Field path for an option's label. Undefined (not "") so PrimeNG applies its own label fallback. */
  public readonly optionLabel = input<string | undefined>(undefined);
  public readonly placeholder = input<string | undefined>(undefined);
  /** PrimeNG default is 1. */
  /**
   * Characters typed before a search fires (p-autocomplete's `minLength`).
   *
   * Deliberately NOT called `minLength`: `FormValueControl` reserves that name for a validator-derived
   * state input, and Signal Forms would auto-bind over it — silently changing when the search triggers.
   */
  public readonly minQueryLength = input<number>(1);
  /** PrimeNG default is 300 (ms). */
  public readonly delay = input<number>(300);
  public readonly id = input<string>("");
  /** PrimeNG signal input `appendTo`, default `undefined`. */
  public readonly appendTo = input<unknown>(undefined);
  /** PrimeNG (BaseInput) default is `undefined`. */
  public readonly fluid = input<boolean | undefined>(undefined);
  public readonly styleClass = input<string | undefined>(undefined);
  /** PrimeNG default is `undefined` (booleanAttribute, no initializer). */
  public readonly forceSelection = input<boolean | undefined>(undefined);
  /** PrimeNG default is `false`. */
  public readonly showClear = input(false, { transform: booleanAttribute });
  /** PrimeNG default is `undefined` (booleanAttribute, no initializer). */
  public readonly dropdown = input<boolean | undefined>(undefined);
}
