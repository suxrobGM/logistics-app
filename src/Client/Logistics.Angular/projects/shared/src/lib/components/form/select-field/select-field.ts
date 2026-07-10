import { Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { Select } from "primeng/select";

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
  imports: [Select, FormsModule],
})
export class UiSelectField<T = unknown> implements FormValueControl<T | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T | null>(null);

  // Optional state inputs. Signal Forms binds these automatically when present;
  // the Reactive Forms bridge drives `disabled`.
  public readonly disabled = input<boolean>(false);
  public readonly readonly = input<boolean>(false);
  public readonly required = input<boolean>(false);
  public readonly invalid = input<boolean>(false);
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  // Presentation
  public readonly options = input.required<readonly unknown[]>();
  public readonly optionLabel = input<string>("");
  public readonly optionValue = input<string>("");
  public readonly placeholder = input<string>("");
  public readonly id = input<string>("");
  public readonly showClear = input<boolean>(false);
  public readonly filter = input<boolean>(false);
  public readonly appendTo = input<unknown>("body");
  public readonly fluid = input<boolean>(true);
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly filterPlaceholder = input<string | undefined>(undefined);
  public readonly filterBy = input<string | undefined>(undefined);
  public readonly loading = input<boolean>(false);
}
