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
 * A digit mask over a plain `<input>`. Replaces `<p-inputmask>` — the last PrimeNG form control in
 * `shared`, and the last `[(ngModel)]` in the library.
 *
 * PRIVATE TO `ui-phone-field`. Not exported from the ui barrel: it implements exactly the slice of
 * `p-inputmask` that the phone field used (`mask`, `slotChar="_"`, `autoClear=false`) and nothing
 * more. A general-purpose mask is a much bigger contract than we need.
 *
 * ------------------------------------------------------------------------------------------------
 * THE MODEL IS THE DIGITS, NOT THE TEXT.
 * ------------------------------------------------------------------------------------------------
 * The single idea that makes this tractable. State is a string of digits ("5551234"); everything the
 * user sees is `render()`ed from it, and everything they type is reduced back to it. There is no
 * attempt to edit the displayed text in place — that is where mask implementations go to die,
 * because every literal in the mask makes caret arithmetic a special case.
 *
 * This also matches the contract `ui-phone-field` already had. `p-inputmask` (with `unmask` unset)
 * pushed the RAW INPUT TEXT into the model — slot chars and all, e.g. `"(555) 12_-____"` — and
 * `PhoneField.emitValue()` did `phone.replace(/\D/g, "")` to recover the digits. So the field only
 * ever cared about digits; this component simply says so, and emits digits directly.
 *
 * ------------------------------------------------------------------------------------------------
 * THE EDGE CASES, and why each line exists.
 * ------------------------------------------------------------------------------------------------
 * - BACKSPACE OVER A LITERAL. In `(555) 123-4567`, backspacing when the caret sits after `)` deletes
 *   the `)`, not a digit. Re-rendering from unchanged digits would restore the `)` and the caret
 *   would jump — the field becomes impossible to clear. So: if a delete left the digit count
 *   unchanged, drop a digit ourselves (`deleteContentBackward` -> the one before the caret,
 *   `deleteContentForward` -> the one after).
 * - PASTE. `insertFromPaste` lands the pasted text in `input.value`; stripping to digits and
 *   re-rendering handles `+1 (555) 123-4567`, `555.123.4567` and `5551234567` identically.
 * - CARET. After every re-render the caret is placed just past the last filled slot, so typing
 *   continues where the user expects instead of at the end of the trailing `___`.
 * - BLUR WITH A PARTIAL VALUE. `p-inputmask` had `[autoClear]="false"`, meaning an incomplete entry
 *   is KEPT on blur, not wiped. (Its default is `true`; the phone field opted out.) An EMPTY value
 *   still clears to "" so the placeholder shows.
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
   * Digits of the selected country's dial code, e.g. `"1"` for +1 — stripped from an OVERLONG paste.
   *
   * Pasting `+1 (555) 123-4567` into a 10-slot US mask yields eleven digits. Filling the mask in
   * order puts the country code's `1` into the first national slot and drops the last digit:
   * `(155) 512-3456`, which composes to `+11555123456` — a phone number that is silently, plausibly
   * WRONG. (`p-inputmask` did exactly this: its `checkVal()` skips non-slot characters and fills the
   * digits in order.) Pasting a fully-qualified number is the normal way people move one between
   * systems, so it is worth getting right.
   *
   * Only applied on OVERFLOW, so a legitimate 10-digit national number that happens to start with 1
   * is never touched.
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
    // Adopt values set from outside (the form loading an existing number), and re-cap when the
    // country — and therefore the mask — changes.
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

    // A delete that removed only a mask literal leaves the digits untouched. Take the digit the user
    // was actually aiming at, or the field can never be emptied past a literal.
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
      // Still force the DOM back in line: the user may have typed a character the mask rejected, and
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
   * Cut a digit string down to what the mask can hold, dropping a leading dial code first when the
   * input overflows — see {@link dialPrefix}. Truncation is the last resort, not the first.
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
