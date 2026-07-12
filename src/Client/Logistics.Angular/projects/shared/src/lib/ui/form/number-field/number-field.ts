import {
  booleanAttribute,
  Component,
  computed,
  ElementRef,
  inject,
  input,
  model,
  output,
  signal,
} from "@angular/core";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { HlmInput } from "../../primitives/input";
import { focusFirstControl } from "../focus-control";

export type NumberFieldMode = "decimal" | "currency";

/**
 * Numeric input built on the native `<input>` + `hlmInput`. Supersedes `ui-currency-field`
 * and `ui-unit-field`: set `mode="currency"` with a `currency` code for money, or a
 * `prefixLabel` / `suffixLabel` addon for unit inputs.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 *
 * Formatting is applied on blur (via `Intl.NumberFormat`) and the raw number is shown while
 * editing — a small, deliberate simplification of `p-inputnumber`'s live caret formatting.
 *
 * @example
 * <ui-form-field label="Rate" for="rate" [required]="true">
 *   <ui-number-field id="rate" [formField]="form.rate" mode="currency" currency="USD" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-number-field",
  templateUrl: "./number-field.html",
  host: { "[attr.id]": "null" },
  imports: [HlmInput],
})
export class UiNumberField implements FormValueControl<number | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<number | null>(null);

  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly touched = input(false, { transform: booleanAttribute });
  public readonly dirty = input(false, { transform: booleanAttribute });
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
  public readonly useGrouping = input(true, { transform: booleanAttribute });

  // Presentation
  public readonly placeholder = input<string>("");
  public readonly id = input<string>("");
  public readonly inputId = input<string>("");
  /** `p-inputnumber` prefix/suffix rendered inside the field, around the number. */
  public readonly prefix = input<string>("");
  public readonly suffix = input<string>("");
  /** Leading / trailing input-group addon (e.g. "$" / "mi"). */
  public readonly prefixLabel = input<string>("");
  public readonly suffixLabel = input<string>("");

  // Kept for call-site API parity with the former p-inputnumber wrapper. The native input is
  // always full-width and has no spinner ramp, so these are no-ops.
  public readonly fluid = input(true, { transform: booleanAttribute });
  public readonly showButtons = input(false, { transform: booleanAttribute });
  public readonly styleClass = input<string>("");

  protected readonly focused = signal(false);

  private readonly formatter = computed(
    () =>
      new Intl.NumberFormat(this.locale() || undefined, {
        style: this.mode() === "currency" ? "currency" : "decimal",
        currency: this.mode() === "currency" ? this.currency() || "USD" : undefined,
        minimumFractionDigits: this.minFractionDigits(),
        maximumFractionDigits:
          this.maxFractionDigits() ?? (this.mode() === "currency" ? 2 : undefined),
        useGrouping: this.useGrouping(),
      }),
  );

  /** Formatted when idle, raw number while editing. */
  protected readonly displayValue = computed(() => {
    const v = this.value();
    if (v === null || v === undefined || Number.isNaN(v)) return "";
    if (this.focused()) return String(v);
    return `${this.prefix()}${this.formatter().format(v)}${this.suffix()}`;
  });

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected onInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value.replace(/[^0-9.-]/g, "");
    if (raw === "" || raw === "-" || raw === ".") {
      this.value.set(null);
      return;
    }
    const n = Number(raw);
    this.value.set(Number.isNaN(n) ? null : n);
  }

  protected onBlur(): void {
    const v = this.value();
    if (v !== null && v !== undefined && !Number.isNaN(v)) {
      const min = this.min();
      const max = this.max();
      let clamped = v;
      if (min !== undefined && clamped < min) clamped = min;
      if (max !== undefined && clamped > max) clamped = max;
      if (clamped !== v) this.value.set(clamped);
    }
    this.focused.set(false);
    this.touch.emit();
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
