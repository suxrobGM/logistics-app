import {
  booleanAttribute,
  Component,
  ElementRef,
  inject,
  input,
  model,
  output,
} from "@angular/core";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { focusFirstControl } from "../focus-control";
import { UiNumberField } from "../number-field/number-field";

/**
 * A currency-prefixed number field — a thin `ui-number-field` configured with a currency addon
 * and two fraction digits.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 * `[formField]` binds this wrapper; the inner `ui-number-field` is driven by a plain two-way
 * `[(value)]`, so there is no dual-binding conflict.
 */
@Component({
  selector: "ui-currency-field",
  templateUrl: "./currency-field.html",
  host: { "[attr.id]": "null" },
  imports: [UiNumberField],
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

  public readonly min = input<number | undefined>(undefined);
  public readonly max = input<number | undefined>(undefined);

  public readonly id = input<string>("");
  public readonly inputId = input<string | undefined>(undefined);
  public readonly currency = input<string>("$");
  public readonly placeholder = input<string>("0.00");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
