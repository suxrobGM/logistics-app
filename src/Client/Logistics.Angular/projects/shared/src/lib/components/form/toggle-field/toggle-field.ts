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
import { ToggleSwitch } from "primeng/toggleswitch";
import { focusFirstControl } from "../../../forms/focus-control";

/**
 * Boolean on/off switch.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The PrimeNG `p-toggleswitch` is itself a `ControlValueAccessor`, so it is driven internally
 * with a standalone `ngModel` (never `formControlName` / `[formField]`, which would collide with
 * Signal Forms' `pattern` state input). The inner `NgModel` lives in THIS wrapper's view, so the
 * enclosing `ui-form-field`'s `contentChild(FORM_FIELD)` still resolves the OUTER binding.
 *
 * `p-toggleswitch` exposes no `onBlur` output (only `onChange`), so `touch` is raised from the
 * native, bubbling `(focusout)` on the host instead.
 *
 * @example
 * <ui-form-field label="Notifications" for="notify">
 *   <ui-toggle-field inputId="notify" formControlName="notify" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-toggle-field",
  templateUrl: "./toggle-field.html",
  imports: [ToggleSwitch, FormsModule],
})
export class UiToggleField implements FormValueControl<boolean> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<boolean>(false);

  // Optional state inputs. Signal Forms binds these automatically when present;
  // the Reactive Forms bridge drives `disabled`.
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

  // Presentation
  public readonly inputId = input<string>("");
  public readonly label = input<string>("");

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
