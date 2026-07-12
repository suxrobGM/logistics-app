import { NgTemplateOutlet } from "@angular/common";
import {
  booleanAttribute,
  Component,
  computed,
  contentChild,
  ElementRef,
  inject,
  input,
  model,
  output,
  signal,
  TemplateRef,
} from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import type { FormValueControl, ValidationError } from "@angular/forms/signals";
import type { BrnOverlayState } from "@spartan-ng/brain/overlay";
import { Subject } from "rxjs";
import { debounceTime, filter } from "rxjs/operators";
import { HlmAutocompleteImports } from "../../primitives/autocomplete";
import { DetachedControl } from "../detached-control";
import { focusFirstControl } from "../focus-control";

/** The payload of `completeMethod` — the current search query. */
export interface UiAutocompleteCompleteEvent {
  query: string;
}

/** Context handed to the `#item` template: the raw suggestion object. */
export interface UiAutocompleteOptionContext<T = unknown> {
  $implicit: T;
}

/**
 * Type-ahead autocomplete.
 *
 * Implements `FormValueControl` only, never a legacy `ControlValueAccessor` — see `text-field.ts`
 * for the bridge contract. The inner `hlm-autocomplete` is driven with plain `[value]` /
 * `(valueChange)` and `(searchChange)`; `uiDetachedControl` severs the ambient `NgControl` so
 * brain's `BrnFieldControl` does not track our Signal Forms control. The panel portals via
 * `*hlmAutocompletePortal`.
 *
 * The parent owns fetching: wire `(completeMethod)` to a search handler and feed `[suggestions]` back.
 *
 * @example
 * <ui-form-field label="Driver" for="driver" [required]="true">
 *   <ui-autocomplete-field id="driver" [formField]="form.driver"
 *     [suggestions]="suggestedDrivers()" optionLabel="fullName" [minQueryLength]="2"
 *     (completeMethod)="searchDriver($event)" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-autocomplete-field",
  templateUrl: "./autocomplete-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  host: { "[attr.id]": "null" },
  imports: [HlmAutocompleteImports, DetachedControl, NgTemplateOutlet],
})
export class UiAutocompleteField<T = unknown> implements FormValueControl<T | null> {
  /**
   * Optional per-suggestion renderer, projected as `<ng-template #item let-suggestion>`; `$implicit`
   * is the raw suggestion object. `descendants: false` so a nested autocomplete keeps its own
   * template instead of having it picked up here.
   */
  protected readonly itemTemplate = contentChild<TemplateRef<UiAutocompleteOptionContext>>("item", {
    descendants: false,
  });

  /** Optional empty-state renderer, `<ng-template #empty>` (e.g. a "create new" action). */
  protected readonly emptyTemplate = contentChild<TemplateRef<unknown>>("empty", {
    descendants: false,
  });
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<T | null>(null);

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

  /** Raised (debounced) when the query is long enough to search. */
  public readonly completeMethod = output<UiAutocompleteCompleteEvent>();

  /** Raised after the user picks a suggestion; emits the chosen value. */
  public readonly optionSelected = output<T | null>();

  /** Raised when the user clears the field. */
  public readonly cleared = output<void>();

  // Presentation
  public readonly suggestions = input.required<readonly T[]>();
  /** Field path for a suggestion's label. Undefined means the suggestion itself is the label. */
  public readonly optionLabel = input<string | undefined>(undefined);
  public readonly placeholder = input<string | undefined>(undefined);
  /**
   * Characters typed before a search fires. Not called `minLength`: `FormValueControl` reserves that
   * name for a validator-derived state input, and Signal Forms would auto-bind over it.
   */
  public readonly minQueryLength = input<number>(1);
  /** Debounce (ms) before `completeMethod` fires. */
  public readonly delay = input<number>(300);
  public readonly id = input<string>("");
  public readonly showClear = input(false, { transform: booleanAttribute });

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly search$ = new Subject<string>();

  constructor() {
    this.search$
      .pipe(
        debounceTime(this.delay()),
        filter((query) => query.length >= this.minQueryLength()),
        takeUntilDestroyed(),
      )
      .subscribe((query) => this.completeMethod.emit({ query }));
  }

  protected label(option: unknown): string {
    const path = this.optionLabel();
    const label = path ? (option as Record<string, unknown>)[path] : option;
    return label == null ? "" : String(label);
  }

  /** Maps the selected value back to its label so the input shows it after a pick. */
  protected readonly itemToString = (value: unknown): string => this.label(value);

  protected onValueChange(next: T | null | undefined): void {
    const value = next ?? null;
    this.value.set(value);
    this.optionSelected.emit(value);
    if (value == null) this.cleared.emit();
  }

  protected onSearch(query: string): void {
    this.search$.next(query);
  }

  protected onClosed(): void {
    this.touch.emit();
  }

  /**
   * Mirrors brain's popover state so `close()` can drive it. The state must be written back on
   * every `stateChanged`, or the `[state]` binding keeps re-applying "closed" and the panel never
   * reopens.
   */
  protected readonly popoverState = signal<BrnOverlayState | null>(null);

  protected onStateChanged(state: BrnOverlayState): void {
    this.popoverState.set(state);
  }

  /**
   * Closes the suggestions panel. A call site that opens a dialog from the `#empty` template must
   * close the panel first, or the overlay survives underneath it.
   */
  public close(): void {
    this.popoverState.set("closed");
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
