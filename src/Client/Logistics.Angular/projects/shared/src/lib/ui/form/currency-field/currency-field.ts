import {
  booleanAttribute,
  Component,
  computed,
  ElementRef,
  inject,
  input,
  model,
  output,
} from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputNumberModule } from "primeng/inputnumber";
import { focusFirstControl } from "../focus-control";

/**
 * A currency-prefixed number field.
 *
 * Implements `FormValueControl` ONLY — never `ControlValueAccessor`. Angular 22 bridges custom
 * Signal Form controls into Reactive and Template-Driven forms with no extra code, so this binds
 * under both `[formField]` and `formControlName`.
 *
 * `[formField]` is never placed on the inner `p-input-number`: it extends PrimeNG's `BaseInput`,
 * whose `pattern: string` collides with the `readonly RegExp[]` state input Signal Forms binds.
 *
 * `min` / `max` are Signal Forms **state inputs**: once bound via `[formField]`, they are driven by
 * the schema's `min()` / `max()` validators, not by the template. Express bounds as validators.
 */
@Component({
  selector: "ui-currency-field",
  templateUrl: "./currency-field.html",
  host: { "[attr.id]": "null" },
  imports: [InputGroupModule, InputGroupAddonModule, InputNumberModule, FormsModule],
})
export class CurrencyField implements FormValueControl<number | null> {
  public readonly value = model<number | null>(null);
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly touched = input(false, { transform: booleanAttribute });
  public readonly dirty = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError.WithOptionalFieldTree[]>([]);
  public readonly name = input<string>("");
  public readonly touch = output<void>();

  // PrimeNG InputNumber declares both as `undefined`; mirror that exactly.
  public readonly min = input<number | undefined>(undefined);
  public readonly max = input<number | undefined>(undefined);

  public readonly id = input<string>("");
  public readonly inputId = input<string | undefined>(undefined);
  public readonly currency = input<string>("$");
  public readonly placeholder = input<string>("0.00");

  /**
   * Signal Forms drives `invalid` from form creation, so a required, untouched field would render
   * as invalid on page load. Reveal it only once the user has interacted — the same rule
   * `ui-form-field` uses for its inline error message.
   */
  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
