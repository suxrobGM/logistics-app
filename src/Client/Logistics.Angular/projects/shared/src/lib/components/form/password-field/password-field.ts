import { booleanAttribute, Component, input, model, output } from "@angular/core";
import { FormsModule } from "@angular/forms";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { Password } from "primeng/password";

/**
 * Masked password input with optional strength feedback and a reveal toggle.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The PrimeNG `p-password` is a `ControlValueAccessor`, so it is driven internally with a
 * standalone `ngModel` (`[ngModel]` / `(ngModelChange)` + `{ standalone: true }`). Never put
 * `formControlName` or `[formField]` on the `p-password` itself: `p-password` extends
 * `BaseInput`, which declares a `pattern: string` input. Signal Forms binds a `pattern`
 * *state* input onto whatever carries `[formField]`, so the two collide — that collision is
 * the whole reason this wrapper exists. The inner NgModel lives in THIS view, not in
 * `ui-form-field`'s projected content, so `ui-form-field`'s `contentChild(NgControl)` still
 * resolves the OUTER binding. See `forms/signal-forms-compat-probe.spec.ts`.
 *
 * @example
 * <ui-form-field label="API Secret" for="apiSecret">
 *   <ui-password-field id="apiSecret" formControlName="apiSecret"
 *     [toggleMask]="true" [feedback]="false" placeholder="Enter API secret" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-password-field",
  templateUrl: "./password-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [Password, FormsModule],
})
export class UiPasswordField implements FormValueControl<string> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<string>("");

  // Optional state inputs. Signal Forms binds these automatically when present;
  // the Reactive Forms bridge drives `disabled`.
  public readonly disabled = input(false, { transform: booleanAttribute });
  // Declared for FormValueControl parity, but NOT forwarded: `p-password` (and its
  // BaseInput/BaseEditableHolder chain) has no `readonly` input in PrimeNG 21.
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised on blur so the form can mark the field touched. */
  public readonly touch = output<void>();

  /** Raised when the user clicks the clear (x) button. Mirrors p-password's `onClear`. */
  public readonly cleared = output<void>();

  // Presentation — every default below mirrors the declared default in
  // node_modules/primeng/fesm2022/primeng-password.mjs (and primeng-baseinput.mjs for `fluid`).
  public readonly id = input<string>("");
  /** Forwarded to the inner input's `inputId`. PrimeNG default: undefined. */
  public readonly placeholder = input<string | undefined>(undefined);
  /** Strength indicator. PrimeNG default: `true`. */
  public readonly feedback = input(true, { transform: booleanAttribute });
  /** Reveal-password icon. PrimeNG default: undefined (falsy). */
  public readonly toggleMask = input<boolean | undefined>(undefined);
  /** BaseInput default: undefined (defers to global inputStyle config). */
  public readonly fluid = input<boolean | undefined>(undefined);
  /** Style class of the host element. PrimeNG default: undefined. */
  public readonly styleClass = input<string | undefined>(undefined);
  /** Style class of the inner input field. PrimeNG default: undefined. */
  public readonly inputStyleClass = input<string | undefined>(undefined);
}
