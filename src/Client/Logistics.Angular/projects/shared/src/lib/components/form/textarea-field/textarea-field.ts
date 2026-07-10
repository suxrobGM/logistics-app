import { Component, input, model, output } from "@angular/core";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { Textarea } from "primeng/textarea";

/**
 * Multi-line text input.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The PrimeNG `pTextarea` is driven internally with plain value/event bindings. Never put
 * `formControlName` or `[formField]` on a PrimeNG element: `pTextarea` crashes on
 * `ngControl.valueChanges`, and every `BaseInput` subclass collides with Signal Forms'
 * `pattern` state input. See `forms/signal-forms-compat-probe.spec.ts`.
 *
 * @example
 * <ui-form-field label="Notes" for="notes" [required]="true">
 *   <ui-textarea-field id="notes" formControlName="notes" placeholder="Details" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-textarea-field",
  templateUrl: "./textarea-field.html",
  imports: [Textarea],
})
export class UiTextareaField implements FormValueControl<string> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<string>("");

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
  public readonly id = input<string>("");
  public readonly rows = input<number>(3);
  public readonly placeholder = input<string>("");
  public readonly maxlength = input<number | null>(null);

  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLTextAreaElement).value);
  }
}
