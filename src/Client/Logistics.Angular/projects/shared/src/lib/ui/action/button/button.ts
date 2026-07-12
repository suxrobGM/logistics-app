import { booleanAttribute, Component, computed, input } from "@angular/core";
import { Icon } from "../../content/icon/icon";
import type { IconName } from "../../icons/icons";
import {
  uiButtonClass,
  type UiButtonAppearance,
  type UiButtonIntent,
  type UiButtonSize,
} from "./button-variants";

/**
 * The button. Replaces `<p-button>` at 496 call sites.
 *
 * WHY IT IS A WRAPPER AND NOT A DIRECTIVE ON A NATIVE `<button>`
 * `<p-button>` is itself a wrapper element that renders a real `<button>` inside. Those 496 hosts
 * sit inside flex rows, grid cells and `gap-*` stacks that were laid out against a wrapper element.
 * Collapsing the DOM to a bare `<button>` would reflow roughly fifty parents in ways no type check
 * or unit test can see. So `ui-button` keeps the wrapper node, and phase 2 is a pure tag rename.
 *
 * WHY THERE IS NO `routerLink` INPUT (and there must never be one)
 * 90 call sites write `<p-button routerLink="/loads">`. `RouterLink`'s selector is `[routerLink]`,
 * which matches the HOST element — it is already bound there today, it already navigates on host
 * click, and it will keep doing exactly that on `<ui-button>` with no work from us. If we ALSO
 * declared an input named `routerLink` and forwarded it to an inner `<a>`, both would bind: the
 * directive on the host and our own inner link. One click, two navigations, on all 90 sites. The
 * attribute stays on the host and this component does not know it exists. Same for `queryParams`.
 *
 * WHY THERE IS NO `<ng-content>`
 * `label` and `icon` inputs are the only way to fill a button. Accepting projected content as well
 * would be two ways to say the same thing, and it buys exactly 3 of the 496 sites (the only ones
 * with a closing `</p-button>` tag). Phase 2 hand-handles those three.
 *
 * WHY THERE IS NO OUTPUT
 * `(click)` bubbles natively from the inner `<button>` to the host, so a call site's `(click)`
 * keeps working. PrimeNG's `(onClick)` emits a plain `MouseEvent` — not a wrapped `{originalEvent}`
 * — so phase 2's `(onClick)` -> `(click)` rename is payload-identical, not merely close enough.
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
    // The inner <button> gets [disabled], which already blocks its own clicks. This stops the HOST
    // from being clickable too — which matters precisely because of RouterLink above: RouterLink
    // listens on the host, so without this a disabled `<ui-button routerLink=...>` would still
    // navigate. It is also what makes a `loading` button un-double-submittable.
    "[class.pointer-events-none]": "disabled() || loading()",
    "[class.w-full]": "block()",
  },
})
export class UiButton {
  public readonly label = input<string>();

  /** Typed against the generated `IconName` union — an unknown icon is a compile error, not a blank. */
  public readonly icon = input<IconName>();

  /** Trailing glyph. Replaces PrimeNG's `iconPos="right"` (16 sites); there is no `iconPos`. */
  public readonly iconEnd = input<IconName>();

  public readonly intent = input<UiButtonIntent>("primary");
  public readonly appearance = input<UiButtonAppearance>("solid");
  public readonly size = input<UiButtonSize>("md");

  /**
   * `<p-button>` has NO type default — it renders `[attr.type]="type || buttonProps?.type"`, so an
   * untyped p-button inside a <form> is a submit button by HTML's own default. We default to
   * "button", which is the safe direction; the 37 sites that say `type="submit"` MUST carry it
   * across, and phase 2 counts them per file to prove none were dropped. A dropped `submit` is a
   * save button that silently does nothing.
   */
  public readonly type = input<"button" | "submit" | "reset">("button");

  public readonly disabled = input(false, { transform: booleanAttribute });

  /** Swaps the icon for a spinner, disables the button and sets aria-busy. All three, in one place. */
  public readonly loading = input(false, { transform: booleanAttribute });

  public readonly rounded = input(false, { transform: booleanAttribute });
  public readonly block = input(false, { transform: booleanAttribute });

  /**
   * The accessible name. MANDATORY when the button is icon-only, which is the only case where there
   * is no text to name it. Phase 2 fills it in at every icon-only site and `check-buttons.mjs`
   * enforces it statically — a runtime warning would only fire on pages someone happened to open.
   */
  public readonly ariaLabel = input<string>();

  /**
   * PrimeNG's `styleClass` (44 sites). It lands on the INNER `<button>`, because that is where
   * PrimeNG put it (`[class]="cn(cx('root'), styleClass, ...)"`). Mapping it to the host `class`
   * instead would look right and silently break layout at every one of those 44 sites.
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
