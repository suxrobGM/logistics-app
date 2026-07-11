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
 * Implements Angular's `FormValueControl` and nothing else — binds via `[formField]`,
 * `formControlName` and `[(ngModel)]` alike, with no `ControlValueAccessor`. The inner control is
 * a real native checkbox driven by plain `[checked]` / `(change)`.
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

  protected onChange(event: Event): void {
    this.value.set((event.target as HTMLInputElement).checked);
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
