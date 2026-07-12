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
import { provideNativeDateAdapter } from "@spartan-ng/brain/date-time";
import { HlmDatePickerImports } from "../../primitives/date-picker";
import { HlmInput } from "../../primitives/input";
import { DetachedControl } from "../detached-control";
import { focusFirstControl } from "../focus-control";

const pad = (value: number): string => String(value).padStart(2, "0");

/**
 * Date (and optional time) picker field.
 *
 * Implements `FormValueControl` only — see `text-field.ts` for the FormValueControl bridge contract.
 *
 * The inner spartan `hlm-date-picker` (brain calendar + `BrnPopover`, native date adapter) is
 * driven with plain `[date]` / `(dateChange)`. `uiDetachedControl` severs the ambient `NgControl`
 * so brain's `BrnFieldControl` does not track our Signal Forms control; invalid styling is owned by
 * `[forceInvalid]="showInvalid()"`. Every call site uses single-date selection; `showTime` /
 * `timeOnly` add a native time input (brain's calendar is date-only).
 *
 * @example
 * <ui-form-field label="Ship date" for="shipDate" [required]="true">
 *   <ui-date-field id="shipDate" [formField]="form.shipDate" [showIcon]="true" />
 * </ui-form-field>
 */
@Component({
  selector: "ui-date-field",
  templateUrl: "./date-field.html",
  // `id` is a declared input, but a static `id="x"` attribute also lands on the host element.
  // Strip it so the id lives only on the inner control and `<label for>` targets something focusable.
  // `focusout` bubbles when focus leaves the field (including into the portaled calendar), which is
  // our blur-equivalent for marking the control touched.
  host: { "[attr.id]": "null", "(focusout)": "onTouched()" },
  imports: [HlmDatePickerImports, HlmInput, DetachedControl],
  providers: [provideNativeDateAdapter()],
})
export class UiDateField implements FormValueControl<Date | null> {
  /** The control's value. Required by `FormValueControl`. */
  public readonly value = model<Date | null>(null);

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

  /** Raised when the user picks a date. Mirrors the old datepicker `onSelect`. */
  public readonly dateSelected = output<Date>();

  // Presentation
  public readonly id = input<string>("");
  public readonly inputId = input<string | undefined>(undefined);
  public readonly showIcon = input(false, { transform: booleanAttribute });
  public readonly dateFormat = input<string>("mm/dd/yy");
  public readonly placeholder = input<string | undefined>(undefined);
  public readonly fluid = input(true, { transform: booleanAttribute });
  public readonly showTime = input(false, { transform: booleanAttribute });
  public readonly timeOnly = input(false, { transform: booleanAttribute });
  public readonly styleClass = input<string | undefined>(undefined);
  /** Retained for call-site API compatibility; the spartan input renders its own trigger icon. */
  public readonly iconDisplay = input<"button" | "input">("button");
  public readonly appendTo = input<unknown>(undefined);
  public readonly minDate = input<Date | undefined>(undefined);
  public readonly maxDate = input<Date | undefined>(undefined);
  /** Only single selection is used across the app; kept for API compatibility. */
  public readonly selectionMode = input<"single" | "multiple" | "range">("single");

  protected readonly showInvalid = computed(
    () => this.invalid() && (this.touched() || this.dirty()),
  );

  /** The resolved id for the inner control, targeted by `<label for>`. */
  protected readonly controlId = computed(() => this.inputId() ?? this.id());

  /** Formats a `Date` for display, honouring the `mm/dd/yy` token format. */
  protected readonly formatDate = (date: Date): string => {
    const format = this.dateFormat();
    const base = format
      .replace(/yy+/g, String(date.getFullYear()))
      .replace(/mm/g, pad(date.getMonth() + 1))
      .replace(/dd/g, pad(date.getDate()));
    return this.showTime() ? `${base} ${this.timeString(date)}` : base;
  };

  /** `HH:MM` for the native time input. */
  protected readonly timeValue = computed(() => {
    const date = this.value();
    return date ? this.timeString(date) : "";
  });

  private timeString(date: Date): string {
    return `${pad(date.getHours())}:${pad(date.getMinutes())}`;
  }

  private readonly host = inject<ElementRef<HTMLElement>>(ElementRef);

  protected onDateChange(next: Date | null): void {
    if (!next) {
      this.value.set(null);
      return;
    }
    // Preserve the current time-of-day when only the calendar date changes.
    const current = this.value();
    if (current && this.showTime()) {
      next.setHours(current.getHours(), current.getMinutes(), 0, 0);
    }
    this.value.set(next);
    this.dateSelected.emit(next);
  }

  protected onTimeInput(event: Event): void {
    const raw = (event.target as HTMLInputElement).value;
    if (!raw) return;
    const [hours, minutes] = raw.split(":").map(Number);
    const base = this.value() ? new Date(this.value()!) : new Date();
    base.setHours(hours, minutes, 0, 0);
    this.value.set(base);
    this.dateSelected.emit(base);
  }

  protected onTouched(): void {
    this.touch.emit();
  }

  /** Signal Forms calls this via `FieldState.focusBoundControl()`. */
  public focus(options?: FocusOptions): void {
    focusFirstControl(this.host.nativeElement, options);
  }
}
