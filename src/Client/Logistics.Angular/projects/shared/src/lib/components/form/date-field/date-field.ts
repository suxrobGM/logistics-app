import { Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { DatePicker } from "primeng/datepicker";

/**
 * Date picker field.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The PrimeNG datepicker is a `ControlValueAccessor`, so it is driven internally with a
 * standalone `ngModel` (never `formControlName`/`[formField]` — every `BaseInput` subclass
 * collides with Signal Forms' `pattern` state input). `standalone: true` keeps the inner
 * NgModel from registering with an ancestor NgForm, and since it lives in this wrapper's
 * own view, `ui-form-field`'s `contentChild(NgControl)` still resolves the outer binding.
 *
 * @example
 * <ui-form-field label="Ship date" for="shipDate" [required]="true">
 *   <ui-date-field id="shipDate" formControlName="shipDate" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-date-field",
  templateUrl: "./date-field.html",
  imports: [DatePicker, FormsModule],
})
export class UiDateField implements FormValueControl<Date | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<Date | null>(null);

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
  public readonly inputId = input<string>("");
  public readonly showIcon = input<boolean>(true);
  public readonly dateFormat = input<string>("mm/dd/yy");
  public readonly placeholder = input<string>("");
  public readonly appendTo = input<unknown>("body");
  public readonly fluid = input<boolean>(true);
  public readonly showTime = input<boolean>(false);
  public readonly timeOnly = input<boolean>(false);
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly iconDisplay = input<"button" | "input" | undefined>(undefined);
  public readonly minDate = input<Date | undefined>(undefined);
  public readonly maxDate = input<Date | undefined>(undefined);
  public readonly selectionMode = input<"single" | "multiple" | "range">("single");
}
