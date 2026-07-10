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
import { Checkbox } from "primeng/checkbox";
import { focusFirstControl } from "../../../forms/focus-control";

/**
 * Binary checkbox.
 *
 * Implements Angular's `FormValueControl<boolean>` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * `p-checkbox` is a PrimeNG `BaseInput` (`ControlValueAccessor`), so it is driven internally with
 * a STANDALONE `NgModel` (`[ngModel]` + `(ngModelChange)` + `{ standalone: true }`). Never put
 * `formControlName` or `[formField]` on the PrimeNG element directly — it collides with Signal
 * Forms' `pattern` state input under strictTemplates. The inner `NgModel` lives in this wrapper's
 * view, so `ui-form-field`'s `contentChild(NgControl)` still resolves the OUTER binding.
 *
 * @example
 * <ui-form-field label="Accept terms" [required]="true">
 *   <ui-checkbox-field inputId="terms" label="I agree" formControlName="terms" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-checkbox-field",
  templateUrl: "./checkbox-field.html",
  imports: [Checkbox, FormsModule],
})
export class UiCheckboxField implements FormValueControl<boolean> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<boolean>(false);

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

  // Presentation
  public readonly inputId = input<string>("");
  public readonly label = input<string>("");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
