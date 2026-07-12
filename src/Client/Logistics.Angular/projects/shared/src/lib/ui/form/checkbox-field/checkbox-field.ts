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
import { Icon } from "../../icons/icon/icon";
import { focusFirstControl } from "../focus-control";

/**
 * Binary checkbox — a native `<input type="checkbox">` (visually hidden, `peer sr-only`) behind a
 * styled box with a lucide check.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 * The inner control is a real native checkbox driven by plain `[checked]` / `(change)`.
 *
 * @example
 * <ui-form-field label="Accept terms" [required]="true">
 *   <ui-checkbox-field inputId="terms" label="I agree" [formField]="form.terms" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-checkbox-field",
  templateUrl: "./checkbox-field.html",
  imports: [Icon],
})
export class UiCheckboxField implements FormValueControl<boolean> {
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
  /**
   * Accessible name for a checkbox whose text sits outside the component (e.g. beside it in the
   * layout) and so cannot be passed as `label`. Without it such a checkbox has no accessible name.
   */
  public readonly ariaLabel = input<string>("");

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
