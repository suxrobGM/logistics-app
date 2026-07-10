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
import { focusFirstControl } from "../../../forms/focus-control";
import { HlmTextarea } from "../../../spartan/textarea";

/**
 * Multi-line text input.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The inner native textarea is styled by spartan's `hlmTextarea` and driven with plain value/event
 * bindings. It used to be PrimeNG's `pTextarea`, the one component in all of primeng that subscribes
 * to `ngControl.valueChanges` and therefore throws under `[formField]`
 * (`forms/signal-forms-compat-probe.spec.ts`, claim C).
 *
 * @example
 * <ui-form-field label="Notes" for="notes" [required]="true">
 *   <ui-textarea-field id="notes" [formField]="form.notes" placeholder="Details" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-textarea-field",
  templateUrl: "./textarea-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [HlmTextarea],
})
export class UiTextareaField implements FormValueControl<string> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<string>("");

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
  public readonly id = input<string>("");
  public readonly rows = input<number>(3);
  public readonly placeholder = input<string>("");
  public readonly maxlength = input<number | null>(null);

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

  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLTextAreaElement).value);
  }
}
