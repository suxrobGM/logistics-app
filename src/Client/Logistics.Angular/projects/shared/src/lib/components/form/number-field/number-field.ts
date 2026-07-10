import { NgTemplateOutlet } from "@angular/common";
import { Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputNumber } from "primeng/inputnumber";

export type NumberFieldMode = "decimal" | "currency";

/**
 * Numeric input. Supersedes `ui-currency-field` and `ui-unit-field`: set `mode="currency"`
 * with a `currency` code for money, or a `prefixLabel` / `suffixLabel` for unit inputs.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * `p-inputnumber` is a PrimeNG `BaseInput` and must NOT receive `formControlName` /
 * `[formField]` directly (its `pattern` state input collides with Signal Forms). It is driven
 * internally with a standalone `NgModel` instead. This inner `NgModel` lives in this wrapper's
 * view, not in `ui-form-field`'s projected content, so `ui-form-field`'s `contentChild(NgControl)`
 * still resolves the OUTER binding.
 *
 * @example
 * <ui-form-field label="Rate" for="rate" [required]="true">
 *   <ui-number-field id="rate" formControlName="rate" mode="currency" currency="USD" prefixLabel="$" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-number-field",
  templateUrl: "./number-field.html",
  imports: [InputNumber, InputGroupModule, InputGroupAddonModule, FormsModule, NgTemplateOutlet],
})
export class UiNumberField implements FormValueControl<number | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<number | null>(null);

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

  // Numeric behaviour
  public readonly min = input<number | undefined>(undefined);
  public readonly max = input<number | undefined>(undefined);
  public readonly mode = input<NumberFieldMode>("decimal");
  public readonly currency = input<string | undefined>(undefined);
  public readonly locale = input<string | undefined>(undefined);
  public readonly minFractionDigits = input<number | undefined>(undefined);
  public readonly maxFractionDigits = input<number | undefined>(undefined);
  public readonly useGrouping = input<boolean>(true);
  public readonly showButtons = input<boolean>(false);

  // Presentation
  public readonly placeholder = input<string>("");
  public readonly id = input<string>("");
  public readonly inputId = input<string>("");
  public readonly fluid = input<boolean>(true);
  public readonly styleClass = input<string | undefined>(undefined);

  /** Leading input-group addon (e.g. "$"). Renders the `p-inputgroup` chrome when set. */
  public readonly prefixLabel = input<string>("");
  /** Trailing input-group addon (e.g. "mi"). Renders the `p-inputgroup` chrome when set. */
  public readonly suffixLabel = input<string>("");
}
