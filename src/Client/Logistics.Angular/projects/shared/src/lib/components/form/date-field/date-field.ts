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
import { DatePicker } from "primeng/datepicker";
import { focusFirstControl } from "../../../forms/focus-control";

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
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [DatePicker, FormsModule],
})
export class UiDateField implements FormValueControl<Date | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<Date | null>(null);

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
   * Mirrors PrimeNG datepicker's `(onSelect)`. Emits the selected `Date` when the user
   * picks a date from the panel. (For single selection PrimeNG emits a `Date`; verified
   * against `primeng/fesm2022/primeng-datepicker.mjs`, `this.onSelect.emit(date)`.)
   */
  public readonly dateSelected = output<Date>();

  // Presentation
  public readonly id = input<string>("");
  public readonly inputId = input<string | undefined>(undefined);
  /** PrimeNG's own default is falsy; templates that want the icon pass [showIcon]="true". */
  public readonly showIcon = input(false, { transform: booleanAttribute });
  public readonly dateFormat = input<string>("mm/dd/yy");
  public readonly placeholder = input<string | undefined>(undefined);
  /** PrimeNG defaults to undefined (inline overlay). Do not portal to body unless asked. */
  public readonly appendTo = input<unknown>(undefined);
  public readonly fluid = input(true, { transform: booleanAttribute });
  public readonly showTime = input(false, { transform: booleanAttribute });
  public readonly timeOnly = input(false, { transform: booleanAttribute });
  public readonly styleClass = input<string | undefined>(undefined);
  /** PrimeNG defaults this to "button". Forwarding `undefined` disables the calendar trigger icon. */
  public readonly iconDisplay = input<"button" | "input">("button");
  public readonly minDate = input<Date | undefined>(undefined);
  public readonly maxDate = input<Date | undefined>(undefined);
  public readonly selectionMode = input<"single" | "multiple" | "range">("single");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
