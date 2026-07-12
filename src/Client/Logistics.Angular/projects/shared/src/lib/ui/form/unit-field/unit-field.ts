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
 * A numeric field with a trailing unit addon (`%`, `mi`, `kg`, …) — a thin `ui-number-field`
 * with a `suffixLabel`.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 * `[formField]` binds this wrapper; the inner `ui-number-field` is driven by a plain two-way
 * `[(value)]`.
 */
@Component({
  selector: "ui-unit-field",
  templateUrl: "./unit-field.html",
  host: { "[attr.id]": "null" },
  imports: [UiNumberField],
})
export class UnitField implements FormValueControl<number | null> {
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
  public readonly unit = input.required<string>();
  public readonly placeholder = input<string>("");

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
