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
import { Icon } from "../../icons/icon/icon";
import { HlmInput } from "../../primitives/input";
import { focusFirstControl } from "../focus-control";

/**
 * Masked password input with a reveal toggle, built on the native `<input>` + `hlmInput`.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 * The inner control is a native element driven by plain value/event bindings, so nothing collides
 * with the `pattern` state input Signal Forms binds onto `[formField]`.
 *
 * @example
 * <ui-form-field label="API Secret" for="apiSecret">
 *   <ui-password-field id="apiSecret" [formField]="form.apiSecret" placeholder="Enter API secret" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-password-field",
  templateUrl: "./password-field.html",
  host: { "[attr.id]": "null" },
  imports: [HlmInput, Icon],
})
export class UiPasswordField implements FormValueControl<string> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<string>("");

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
  public readonly id = input<string>("");
  public readonly placeholder = input<string>("");
  public readonly autocomplete = input<string>("current-password");
  public readonly maxlength = input<number | undefined>(undefined);
  /** Whether to show the reveal (eye) button. */
  public readonly toggleMask = input(true, { transform: booleanAttribute });
  /**
   * Kept for API parity with call sites. The native input has no strength meter (these
   * are secret fields), so this is a no-op.
   */
  public readonly feedback = input(false, { transform: booleanAttribute });

  protected readonly revealed = signal(false);

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLInputElement).value);
  }

  protected toggleReveal(): void {
    this.revealed.update((v) => !v);
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
