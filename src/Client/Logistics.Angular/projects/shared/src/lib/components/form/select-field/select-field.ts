import {
  booleanAttribute,
  Component,
  ElementRef,
  inject,
  input,
  model,
  output,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { Select } from "primeng/select";
import { focusFirstControl } from "../../../forms/focus-control";

/**
 * Single-select dropdown.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The PrimeNG `p-select` is a `ControlValueAccessor`, so it is driven internally with a
 * standalone `ngModel` (`[ngModel]` / `(ngModelChange)` + `{ standalone: true }`). Never put
 * `formControlName` or `[formField]` on the `p-select` itself: every `BaseInput` subclass
 * collides with Signal Forms' `pattern` state input. The inner NgModel lives in THIS view,
 * not in `ui-form-field`'s projected content, so `ui-form-field`'s `contentChild(NgControl)`
 * still resolves the OUTER binding. See `forms/signal-forms-compat-probe.spec.ts`.
 *
 * @example
 * <ui-form-field label="Color" for="color" [required]="true">
 *   <ui-select-field id="color" formControlName="color"
 *     [options]="colors" optionLabel="label" optionValue="value" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-select-field",
  templateUrl: "./select-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [Select, FormsModule],
})
export class UiSelectField<T = unknown> implements FormValueControl<T | null> {
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

  /** Raised after the user picks an option. Mirrors p-select's `onChange`. */
  public readonly selectionChange = output<T | null>();

  /** Raised when the user clicks the clear (x) button. Mirrors p-select's `onClear`. */
  public readonly cleared = output<void>();

  // Presentation
  public readonly options = input.required<readonly unknown[]>();
  /** Field path for an option's label. Undefined (not "") so PrimeNG applies its own fallback. */
  public readonly optionLabel = input<string | undefined>(undefined);
  /** Field path for an option's value. Undefined (not "") — an empty path resolves to undefined. */
  public readonly optionValue = input<string | undefined>(undefined);
  public readonly placeholder = input<string | undefined>(undefined);
  public readonly id = input<string>("");
  public readonly showClear = input(false, { transform: booleanAttribute });
  public readonly filter = input(false, { transform: booleanAttribute });
  /** PrimeNG defaults to undefined (inline overlay). Do not portal to body unless asked. */
  public readonly appendTo = input<unknown>(undefined);
  public readonly fluid = input(true, { transform: booleanAttribute });
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly filterPlaceholder = input<string | undefined>(undefined);
  public readonly filterBy = input<string | undefined>(undefined);
  public readonly loading = input(false, { transform: booleanAttribute });

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
