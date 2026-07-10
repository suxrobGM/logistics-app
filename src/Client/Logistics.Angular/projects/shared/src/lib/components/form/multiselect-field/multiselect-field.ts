import { Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { MultiSelect } from "primeng/multiselect";

/**
 * Multi-select dropdown backed by PrimeNG's `p-multiselect`.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * `p-multiselect` is a `ControlValueAccessor` (a `BaseEditableHolder`/`BaseInput` subclass), so
 * we must NOT put `formControlName` / `[formField]` on it — that would collide with Signal Forms'
 * `pattern` state input under strictTemplates. Instead we drive it with a standalone `NgModel`
 * (`[ngModel]` + `(ngModelChange)`); the `standalone: true` keeps that inner NgModel from
 * registering with an ancestor `NgForm`, and it lives in THIS wrapper's view so
 * `ui-form-field`'s `contentChild(NgControl)` still resolves the OUTER binding.
 *
 * @example
 * <ui-form-field label="Tags" for="tags" [required]="true">
 *   <ui-multiselect-field id="tags" formControlName="tags"
 *     [options]="tagOptions" optionLabel="label" optionValue="value" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-multiselect-field",
  templateUrl: "./multiselect-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [MultiSelect, FormsModule],
})
export class UiMultiSelectField<T = unknown> implements FormValueControl<T[]> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T[]>([]);

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
  public readonly options = input.required<unknown[]>();
  public readonly optionLabel = input<string>();
  public readonly optionValue = input<string>();
  public readonly placeholder = input<string>("");
  public readonly showClear = input<boolean>(false);
  public readonly filter = input<boolean>(false);
  public readonly filterBy = input<string>();
  public readonly display = input<"comma" | "chip">("comma");
  public readonly maxSelectedLabels = input<number>();
  public readonly id = input<string>("");
  public readonly appendTo = input<unknown>("body");
}
