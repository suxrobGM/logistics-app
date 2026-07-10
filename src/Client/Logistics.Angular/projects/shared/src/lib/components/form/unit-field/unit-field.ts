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
import { InputGroupModule } from "primeng/inputgroup";
import { InputGroupAddonModule } from "primeng/inputgroupaddon";
import { InputTextModule } from "primeng/inputtext";
import { focusFirstControl } from "../../../forms/focus-control";

/**
 * A numeric field with a trailing unit addon (`%`, `mi`, `kg`, …).
 *
 * Implements `FormValueControl` ONLY — never `ControlValueAccessor`. `[formField]` binds to this
 * wrapper, never to the inner element.
 *
 * `pInputText` is a directive on a native `<input>`, so it carries no `BaseInput` `pattern`
 * collision. The value is parsed here rather than left as the raw string a native `type="number"`
 * input reports — under the old `[formControl]` binding the `DefaultValueAccessor` wrote strings
 * into a control typed `number | null`.
 *
 * `min` / `max` are Signal Forms **state inputs**: once bound via `[formField]`, they are driven by
 * the schema's `min()` / `max()` validators, not by the template. Express bounds as validators.
 */
@Component({
  selector: "ui-unit-field",
  templateUrl: "./unit-field.html",
  host: { "[attr.id]": "null" },
  imports: [InputGroupModule, InputGroupAddonModule, InputTextModule],
})
export class UnitField implements FormValueControl<number | null> {
  public readonly value = model<number | null>(null);
  public readonly disabled = input(false, { transform: booleanAttribute });
  public readonly readonly = input(false, { transform: booleanAttribute });
  public readonly required = input(false, { transform: booleanAttribute });
  public readonly invalid = input(false, { transform: booleanAttribute });
  public readonly touched = input(false, { transform: booleanAttribute });
  public readonly dirty = input(false, { transform: booleanAttribute });
  public readonly errors = input<readonly ValidationError.WithOptionalFieldTree[]>([]);
  public readonly name = input<string>("");
  public readonly touch = output<void>();

  public readonly min = input<number | undefined>(undefined);
  public readonly max = input<number | undefined>(undefined);

  public readonly id = input<string>("");
  public readonly unit = input.required<string>();
  public readonly placeholder = input<string>("");

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
    const raw = (event.target as HTMLInputElement).value;
    this.value.set(raw === "" ? null : Number(raw));
  }
}
