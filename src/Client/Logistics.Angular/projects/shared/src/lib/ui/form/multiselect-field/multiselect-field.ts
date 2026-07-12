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
import { HlmSelectImports } from "../../primitives/select";
import { DetachedControl } from "../detached-control";
import { focusFirstControl } from "../focus-control";

/**
 * Multi-select dropdown.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 *
 * The inner spartan `hlm-select-multiple` (brain `BrnSelectMultiple` + `BrnPopover`) is driven with
 * plain `[value]` / `(valueChange)`. `uiDetachedControl` severs the ambient `NgControl` so brain's
 * `BrnFieldControl` does not track our Signal Forms control. The panel portals via `*hlmSelectPortal`.
 *
 * @example
 * <ui-form-field label="Tags" for="tags" [required]="true">
 *   <ui-multiselect-field id="tags" [formField]="form.tags"
 *     [options]="tagOptions" optionLabel="label" optionValue="value" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-multiselect-field",
  templateUrl: "./multiselect-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [HlmSelectImports, DetachedControl, Icon],
})
export class UiMultiSelectField<T = unknown> implements FormValueControl<T[]> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T[]>([]);

  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly touched = input(false, { transform: booleanAttribute });
  public readonly dirty = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError[]>([]);
  public readonly name = input<string>("");

  /** Raised when the panel closes so the form can mark the field touched. */
  public readonly touch = output<void>();

  /** Raised after the selection changes. */
  public readonly selectionChange = output<T[]>();

  /** Raised when the user clears the whole selection. */
  public readonly cleared = output<void>();

  // Presentation
  public readonly options = input.required<unknown[]>();
  public readonly optionLabel = input<string>();
  public readonly optionValue = input<string>();
  public readonly placeholder = input<string | undefined>(undefined);
  public readonly showClear = input(false, { transform: booleanAttribute });
  public readonly filter = input(false, { transform: booleanAttribute });
  public readonly filterBy = input<string>();
  public readonly display = input<"comma" | "chip">("comma");
  public readonly maxSelectedLabels = input<number>();
  public readonly id = input<string>("");
  public readonly fluid = input(true, { transform: booleanAttribute });

  /** Free-text filter over the visible options, active only when `filter` is set. */
  protected readonly filterText = signal("");

  protected readonly displayOptions = computed(() => {
    const opts = this.options();
    const query = this.filterText().trim().toLowerCase();
    if (!this.filter() || !query) return opts;
    return opts.filter((opt) => this.resolveLabel(opt).toLowerCase().includes(query));
  });

  /** The selected options, resolved back from the stored values for chip display. */
  protected readonly selectedLabels = computed(() =>
    this.value().map((v) => ({ value: v, label: this.itemToString(v) })),
  );

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

  /** Maps a stored value back to its label (used by `hlm-select-value` and chips). */
  protected readonly itemToString = (value: unknown): string => {
    const option = this.options().find((opt) => this.resolveValue(opt) === value);
    if (option !== undefined) return this.resolveLabel(option);
    return value == null ? "" : String(value);
  };

  protected onValueChange(next: T[] | null | undefined): void {
    const value = next ?? [];
    this.value.set(value);
    this.selectionChange.emit(value);
  }

  protected onClosed(): void {
    this.filterText.set("");
    this.touch.emit();
  }

  /** Fired by both click and Enter/Space, so suppress the key's default (page scroll on Space). */
  protected clear(event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    this.value.set([]);
    this.cleared.emit();
  }

  /** Fired by both click and Enter/Space, so suppress the key's default (page scroll on Space). */
  protected removeChip(value: T, event: Event): void {
    event.stopPropagation();
    event.preventDefault();
    const next = this.value().filter((v) => v !== value);
    this.value.set(next);
    this.selectionChange.emit(next);
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
