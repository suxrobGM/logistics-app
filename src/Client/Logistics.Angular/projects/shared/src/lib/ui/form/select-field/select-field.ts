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
import { Icon } from "../../content/icon/icon";
import { HlmSelectImports } from "../../primitives/select";
import { DetachedControl } from "../detached-control";
import { focusFirstControl } from "../focus-control";

/**
 * Single-select dropdown.
 *
 * Implements Angular's `FormValueControl` and nothing else. Angular 22 bridges custom
 * signal-form controls into Reactive and Template-Driven forms automatically, so this one
 * component binds via `[formField]`, `formControlName` and `[(ngModel)]` alike — no
 * `ControlValueAccessor`, no compat shim.
 *
 * The inner spartan `hlm-select` (brain `BrnSelect` + `BrnPopover`) is driven with plain
 * `[value]` / `(valueChange)` — never `formControlName` / `[formField]` on it. `uiDetachedControl`
 * severs the ambient `NgControl` so brain's `BrnFieldControl` does not track our Signal Forms
 * control; invalid styling is owned by `[forceInvalid]="showInvalid()"` on the trigger. The panel
 * portals via `*hlmSelectPortal` (brain's `BrnPopoverContent`) — without it the overlay never opens.
 *
 * @example
 * <ui-form-field label="Color" for="color" [required]="true">
 *   <ui-select-field id="color" formControlName="color"
 *     [options]="colors" optionLabel="label" optionValue="value" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-select-field",
  templateUrl: "./select-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [HlmSelectImports, DetachedControl, Icon],
})
export class UiSelectField<T = unknown> implements FormValueControl<T | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T | null>(null);

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

  /** Raised when the field is blurred (the panel closes) so the form can mark it touched. */
  public readonly touch = output<void>();

  /** Raised after the user picks an option. Mirrors the old p-select `onChange`. */
  public readonly selectionChange = output<T | null>();

  /** Raised when the user clicks the clear (x) button. Mirrors the old p-select `onClear`. */
  public readonly cleared = output<void>();

  // Presentation
  public readonly options = input.required<readonly unknown[]>();
  /** Field path for an option's label. Undefined means the option itself is the label. */
  public readonly optionLabel = input<string | undefined>(undefined);
  /** Field path for an option's value. Undefined means the option itself is the value. */
  public readonly optionValue = input<string | undefined>(undefined);
  public readonly placeholder = input<string | undefined>(undefined);
  public readonly id = input<string>("");
  public readonly showClear = input(false, { transform: booleanAttribute });
  public readonly filter = input(false, { transform: booleanAttribute });
  public readonly fluid = input(true, { transform: booleanAttribute });
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly filterPlaceholder = input<string | undefined>(undefined);
  public readonly filterBy = input<string | undefined>(undefined);
  public readonly loading = input(false, { transform: booleanAttribute });

  /** Free-text filter over the visible options, active only when `filter` is set. */
  protected readonly filterText = signal("");

  protected readonly displayOptions = computed(() => {
    const opts = this.options();
    const query = this.filterText().trim().toLowerCase();
    if (!this.filter() || !query) return opts;
    return opts.filter((opt) => this.resolveLabel(opt).toLowerCase().includes(query));
  });

  /**
   * Signal Forms drives `invalid` from form creation, so a required, untouched field would render
   * as invalid on page load. Reveal it only once the user has interacted — the same rule
   * `ui-form-field` uses for its inline error message.
   */
  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected resolveValue(option: unknown): unknown {
    const path = this.optionValue();
    return path ? (option as Record<string, unknown>)[path] : option;
  }

  protected resolveLabel(option: unknown): string {
    const path = this.optionLabel();
    const label = path ? (option as Record<string, unknown>)[path] : option;
    return label == null ? "" : String(label);
  }

  /** Maps a stored value back to its label so `hlm-select-value` renders the selection. */
  protected readonly itemToString = (value: unknown): string => {
    const option = this.options().find((opt) => this.resolveValue(opt) === value);
    if (option !== undefined) return this.resolveLabel(option);
    return value == null ? "" : String(value);
  };

  protected onValueChange(next: T | null | undefined): void {
    const value = next ?? null;
    this.value.set(value);
    this.selectionChange.emit(value);
  }

  protected onClosed(): void {
    this.filterText.set("");
    this.touch.emit();
  }

  protected clear(event: Event): void {
    event.stopPropagation();
    this.value.set(null);
    this.cleared.emit();
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
