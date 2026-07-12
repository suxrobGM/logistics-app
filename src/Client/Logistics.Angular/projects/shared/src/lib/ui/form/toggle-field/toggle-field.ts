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
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import { focusFirstControl } from "../focus-control";

/**
 * Boolean on/off switch — a native `<input type="checkbox">` (visually hidden, `peer sr-only`)
 * behind a styled track + thumb.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 * The inner control is a real native checkbox driven by plain `[checked]` / `(change)`.
 *
 * @example
 * <ui-form-field label="Notifications" for="notify">
 *   <ui-toggle-field inputId="notify" [formField]="form.notify" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-toggle-field",
  templateUrl: "./toggle-field.html",
})
export class UiToggleField implements FormValueControl<boolean> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<boolean>(false);

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

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected onChange(event: Event): void {
    this.value.set((event.target as HTMLInputElement).checked);
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
