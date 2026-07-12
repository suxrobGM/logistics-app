import { booleanAttribute, Component, computed, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import type { IconName } from "../../icons/icons";
import {
  uiButtonClass,
  type UiButtonAppearance,
  type UiButtonIntent,
  type UiButtonSize,
} from "./button-variants";

/**
 * The button. A wrapper element around a real `<button>`, not a directive on a native one — call
 * sites lay out against the wrapper node, so collapsing the DOM would reflow their parents.
 *
 * There must NEVER be a `routerLink` input. `RouterLink`'s selector is `[routerLink]`, so it already
 * matches the HOST and already navigates on host click. Declaring our own input of that name and
 * forwarding it to an inner `<a>` would bind both — one click, two navigations. Same for
 * `queryParams`. The attribute stays on the host and this component does not know it exists.
 *
 * There is no `<ng-content>` (`label` / `icon` are the only way to fill a button) and no output:
 * `(click)` bubbles natively from the inner `<button>` with a plain `MouseEvent` payload.
 *
 * @example
 * <ui-button label="Save" icon="check" type="submit" [loading]="saving()" />
 * <ui-button icon="trash" intent="danger" appearance="text" ariaLabel="Delete load" (click)="del()" />
 */
@Component({
  selector: "ui-button",
  templateUrl: "./button.html",
  imports: [Icon],
  host: {
    class: "inline-flex",
    // The inner <button>'s own [disabled] blocks its clicks; this kills the HOST's. Load-bearing
    // because RouterLink listens on the host — without it a disabled `<ui-button routerLink=…>`
    // would still navigate, and a `loading` button would still be double-submittable.
    "[class.pointer-events-none]": "disabled() || loading()",
    "[class.w-full]": "block()",
  },
})
export class UiButton {
  public readonly label = input<string>();

  /** Typed against the generated `IconName` union — an unknown icon is a compile error, not a blank. */
  public readonly icon = input<IconName>();

  /** Trailing glyph. There is no `iconPos` input — use this instead. */
  public readonly iconEnd = input<IconName>();

  public readonly intent = input<UiButtonIntent>("primary");
  public readonly appearance = input<UiButtonAppearance>("solid");
  public readonly size = input<UiButtonSize>("md");

  /**
   * Defaults to "button", NOT to HTML's own default of "submit". A button inside a `<form>` that
   * means to submit must say so explicitly.
   */
  public readonly type = input<"button" | "submit" | "reset">("button");

  public readonly disabled = input(false, { transform: booleanAttribute });

  /** Swaps the icon for a spinner, disables the button and sets aria-busy — all three. */
  public readonly loading = input(false, { transform: booleanAttribute });

  public readonly rounded = input(false, { transform: booleanAttribute });
  public readonly block = input(false, { transform: booleanAttribute });

  /**
   * The accessible name. MANDATORY when the button is icon-only — there is no text to name it.
   * `check-buttons.mjs` enforces that statically.
   */
  public readonly ariaLabel = input<string>();

  /**
   * Extra classes for the INNER `<button>`, which is what call sites lay out against. Mapping it to
   * the host `class` instead would look right and silently break their layout.
   */
  public readonly buttonClass = input<string>("");

  /** No label to name the button, so the glyph is all there is: render a square. */
  protected readonly iconOnly = computed(
    () => !this.label() && (!!this.icon() || !!this.iconEnd() || this.loading()),
  );

  protected readonly isDisabled = computed(() => this.disabled() || this.loading());

  protected readonly classes = computed(() =>
    uiButtonClass({
      intent: this.intent(),
      appearance: this.appearance(),
      size: this.size(),
      iconOnly: this.iconOnly(),
      rounded: this.rounded(),
      block: this.block(),
      extra: this.buttonClass(),
    }),
  );
}
