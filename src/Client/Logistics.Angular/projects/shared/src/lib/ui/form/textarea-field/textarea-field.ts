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
import { HlmTextarea } from "../../primitives/textarea";
import { focusFirstControl } from "../focus-control";

/**
 * Multi-line text input.
 *
 * Implements `FormValueControl` only - see `text-field.ts` for the FormValueControl bridge contract.
 * The inner native textarea is styled by spartan's `hlmTextarea` and driven with plain value/event
 * bindings.
 *
 * @example
 * <ui-form-field label="Notes" for="notes" [required]="true">
 *   <ui-textarea-field id="notes" [formField]="form.notes" placeholder="Details" />
 * </ui-form-field>
 *
 * @example Opt into the remaining-characters counter:
 * <ui-textarea-field [formField]="form.notes" [maxlength]="500" showCounter />
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

  /** Renders a "remaining / max" counter under the textarea. Ignored without a `maxlength`. */
  public readonly showCounter = input(false, { transform: booleanAttribute });

  /**
   * Caps the always-on content sizing (hlmTextarea's `field-sizing-content`) at `max-h-40`, so a
   * long draft scrolls instead of eating the panel.
   */
  public readonly capAutoGrow = input(false, { transform: booleanAttribute });

  /** Remaining characters at or below which the counter turns destructive. */
  public readonly counterWarnAt = input<number>(50);

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  /** Null when the counter is off or there is no maxlength to count against. */
  protected readonly counter = computed(() => {
    const max = this.maxlength();
    if (!this.showCounter() || max === null) {
      return null;
    }

    const remaining = max - this.value().length;
    return { remaining, max, warn: remaining <= this.counterWarnAt() };
  });

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }

  protected onInput(event: Event): void {
    this.value.set((event.target as HTMLTextAreaElement).value);
  }
}
