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
import { HlmInput } from "../../primitives/input";
import { focusFirstControl } from "../focus-control";

export type TextFieldType = "text" | "email" | "password" | "tel" | "url" | "search";

/**
 * Single-line text input.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]` with no value-accessor glue and no compat
 * shim.
 *
 * The inner native input is styled by spartan's `hlmInput` and driven with plain value/event
 * bindings. `[forceInvalid]` feeds brain's `data-matches-spartan-invalid` styling hook, which is
 * what paints the destructive ring.
 *
 * @example
 * <ui-form-field label="Name" for="name" [required]="true">
 *   <ui-text-field id="name" [formField]="form.name" placeholder="Full name" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-text-field",
  templateUrl: "./text-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [HlmInput],
})
export class UiTextField implements FormValueControl<string> {
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
  public readonly type = input<TextFieldType>("text");
  public readonly placeholder = input<string>("");
  public readonly autocomplete = input<string>("");
  public readonly maxlength = input<number | undefined>(undefined);

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
    this.value.set((event.target as HTMLInputElement).value);
  }
}
