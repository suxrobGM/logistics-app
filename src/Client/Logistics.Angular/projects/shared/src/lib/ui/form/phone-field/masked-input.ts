import {
  booleanAttribute,
  Component,
  computed,
  effect,
  ElementRef,
  input,
  output,
  signal,
  untracked,
  viewChild,
} from "@angular/core";
import { HlmInputGroupInput } from "../../primitives/input-group";

/** Mask token for "one digit here". Every other character in a mask is a literal. */
const DIGIT = "9";

/**
 * A digit mask over a plain `<input>`. Private to `ui-phone-field`, not in the ui barrel.
 *
 * The model is the digit string ("5551234"), never the rendered text: the display is `render()`ed
 * from the digits and every input event is reduced back to them. Nothing edits the displayed text in
 * place, which keeps caret arithmetic from becoming a special case per mask literal.
 *
 * Consequences handled below: a delete that only removed a mask literal leaves the digit count
 * unchanged, so we drop the digit the user aimed at (or the field can never be cleared past a
 * literal); a paste of any format is stripped to digits; after every re-render the caret is re-parked
 * just past the last filled slot, so typing continues where the user expects rather than after the
 * trailing slot chars; a partial value survives blur, while an empty one renders "" so the
 * placeholder shows.
 */
@Component({
  selector: "ui-masked-input",
  templateUrl: "./masked-input.html",
  imports: [HlmInputGroupInput],
})
export class MaskedInput {
  public readonly mask = input.required<string>();
  /** Incoming digits. Anything non-digit is ignored, so an E.164 tail or a formatted number works. */
  public readonly value = input<string>("");
  public readonly placeholder = input<string>("");
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly slotChar = input<string>("_");

  /**
   * Digits of the selected country's dial code, e.g. `"1"` for +1. Stripped only when a paste
   * overflows the mask: `+1 (555) 123-4567` is eleven digits, and filling a 10-slot US mask in order
   * would shift the country code into the first national slot and drop the last digit — a silently
   * plausible wrong number. Overflow-only, so a national number starting with 1 is never touched.
   */
  public readonly dialPrefix = input<string>("");

  /** Emits the DIGITS only — never the rendered text. */
  public readonly valueChange = output<string>();
  public readonly blurred = output<void>();

  private readonly inputRef = viewChild.required<ElementRef<HTMLInputElement>>("input");

  private readonly digits = signal("");
  private readonly focused = signal(false);

  /** How many digits this mask accepts. */
  private readonly capacity = computed(() => this.count(this.mask(), DIGIT));

  /**
   * What the input shows. Empty (so the placeholder is visible) only when there is nothing to show
   * AND the field is not focused — focusing an empty field reveals the slots to type into.
   */
  protected readonly display = computed(() => {
    const digits = this.digits();
    if (digits.length === 0 && !this.focused()) {
      return "";
    }
    return this.render(digits);
  });

  constructor() {
    // Adopt values set from outside, and re-fit when the country (and so the mask) changes.
    effect(() => {
      const incoming = this.fit(this.onlyDigits(this.value()));
      untracked(() => {
        if (incoming !== this.digits()) {
          this.digits.set(incoming);
        }
      });
    });

    // Keep the DOM value in step with `display()`, and park the caret after the last filled slot.
    effect(() => {
      const display = this.display();
      const digits = this.digits();
      const element = this.inputRef().nativeElement;
      if (element.value === display) {
        return;
      }
      element.value = display;
      if (this.focused()) {
        const caret = this.caretFor(digits.length);
        element.setSelectionRange(caret, caret);
      }
    });
  }

  protected onInput(event: Event): void {
    const element = event.target as HTMLInputElement;
    const inputType = (event as InputEvent).inputType;
    const before = this.digits();

    let next = this.fit(this.onlyDigits(element.value));

    // A delete that removed only a mask literal leaves the digits untouched, so drop the digit the
    // user was aiming at.
    if (next.length === before.length && next.length > 0) {
      if (inputType === "deleteContentBackward") {
        next = next.slice(0, -1);
      } else if (inputType === "deleteContentForward") {
        const at = this.digitIndexAt(element.selectionStart ?? 0);
        next = next.slice(0, at) + next.slice(at + 1);
      }
    }

    this.commit(next);
  }

  protected onFocus(): void {
    this.focused.set(true);
  }

  protected onBlur(): void {
    this.focused.set(false);
    this.blurred.emit();
  }

  private commit(digits: string): void {
    if (digits === this.digits()) {
      // Force the DOM back in line anyway: the user may have typed a character the mask rejected and
      // the browser has already painted it.
      this.inputRef().nativeElement.value = this.display();
      const caret = this.caretFor(digits.length);
      this.inputRef().nativeElement.setSelectionRange(caret, caret);
      return;
    }
    this.digits.set(digits);
    this.valueChange.emit(digits);
  }

  /** Fill the mask's slots with `digits`, padding what is left with the slot char. */
  private render(digits: string): string {
    let out = "";
    let d = 0;
    for (const char of this.mask()) {
      if (char === DIGIT) {
        out += d < digits.length ? digits[d++] : this.slotChar();
      } else {
        out += char;
      }
    }
    return out;
  }

  /** Offset in the rendered string immediately after the `n`th slot (or at the first, when n = 0). */
  private caretFor(n: number): number {
    const mask = this.mask();
    let seen = 0;
    for (let i = 0; i < mask.length; i++) {
      if (mask[i] !== DIGIT) {
        continue;
      }
      if (seen === n) {
        return i;
      }
      seen++;
      if (seen === n) {
        return i + 1;
      }
    }
    return mask.length;
  }

  /** How many slots sit before `offset` in the rendered string — i.e. which digit the caret is on. */
  private digitIndexAt(offset: number): number {
    return this.count(this.mask().slice(0, offset), DIGIT);
  }

  /**
   * Cut a digit string down to what the mask can hold, dropping a leading dial code before falling
   * back to truncation — see {@link dialPrefix}.
   */
  private fit(digits: string): string {
    const capacity = this.capacity();
    if (digits.length <= capacity) {
      return digits;
    }
    const prefix = this.dialPrefix();
    if (prefix && digits.startsWith(prefix) && digits.length - prefix.length <= capacity) {
      return digits.slice(prefix.length);
    }
    return digits.slice(0, capacity);
  }

  private onlyDigits(value: string): string {
    return (value ?? "").replace(/\D/g, "");
  }

  private count(haystack: string, needle: string): number {
    let n = 0;
    for (const char of haystack) {
      if (char === needle) {
        n++;
      }
    }
    return n;
  }
}
