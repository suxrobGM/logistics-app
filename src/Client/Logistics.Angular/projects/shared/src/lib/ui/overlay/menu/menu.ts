import { Component, computed, input, viewChild, type TemplateRef } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Icon } from "../../icons/icon/icon";
import { AnchoredOverlay, BELOW_TRIGGER_POSITIONS } from "../../internal/anchored-overlay";
import {
  HlmDropdownMenu,
  HlmDropdownMenuItem,
  HlmDropdownMenuSeparator,
} from "../../primitives/dropdown-menu";
import type { UiMenuItem } from "./menu-item";

/**
 * The popup menu — the same row-kebab pattern at every call site, and the only consumer of the Helm
 * dropdown-menu primitive.
 *
 * THE TRIGGER CONTRACT:
 *   <ui-button icon="ellipsis-vertical" (click)="selectedRow.set(row); menu.toggle($event)" />
 *   <ui-menu #menu [items]="actionMenuItems()" />
 *
 * The imperative `toggle($event)` is not a stylistic hangover, it is load-bearing. The call sites run
 * `selectedRow.set(row)` and *then* open, and `actionMenuItems()` is computed from that row. Swapping
 * in a declarative `[uiMenuTriggerFor]` directive would put CDK's own click handler in a race with
 * that setter on the same element, and a menu built from a stale row navigates to the wrong record.
 * See `AnchoredOverlay` for why that also rules out `CdkMenuTrigger` and its overlay.
 *
 * WHY THE CLOSE PATHS ARE OURS
 * `CdkMenu` sets `isInline = !this._parentTrigger`. With no `CdkMenuTrigger` above it there is no
 * `MENU_TRIGGER`, so it considers itself "inline" — and an inline menu NEVER PUSHES ITSELF ONTO THE
 * MENU STACK (`ngAfterContentInit`: `if (!this.isInline) this.menuStack.push(this)`). Its Escape
 * handler then calls `menuStack.close(this)`, whose first act is an `indexOf(this) >= 0` test that is
 * false, so it is a SILENT NO-OP: trusting CDK's stack here gives a menu that opens and never closes on
 * Escape, with no error anywhere. `AnchoredOverlay` owns Escape / outside-click / navigation /
 * scroll-away; this class adds the two menu-specific ones (item activation, tab-out).
 *
 * What `CdkMenu` still gives us is the genuinely hard part: roving-tabindex arrow keys, type-ahead and
 * Home/End. Focusing the menu container on open is enough to start it — `CdkMenuBase._handleFocus()`
 * calls `focusFirstItem()` for any non-mouse focus origin.
 */
@Component({
  selector: "ui-menu",
  templateUrl: "./menu.html",
  imports: [HlmDropdownMenu, HlmDropdownMenuItem, HlmDropdownMenuSeparator, RouterLink, Icon],
})
export class UiMenu {
  public readonly items = input<readonly UiMenuItem[]>([]);
  public readonly ariaLabel = input<string | undefined>(undefined);

  private readonly panel = viewChild.required<TemplateRef<unknown>>("panel");

  private readonly overlay = new AnchoredOverlay(() => this.panel(), BELOW_TRIGGER_POSITIONS);

  /** `visible: false` removes an item; `undefined` means visible, exactly as `MenuItem` behaved. */
  protected readonly visibleItems = computed(() =>
    this.items().filter((item) => item.visible !== false),
  );

  public isOpen(): boolean {
    return this.overlay.opened();
  }

  /** The call sites' entry point: `(click)="menu.toggle($event)"`. */
  public toggle(event: Event): void {
    const wasOpen = this.overlay.opened();
    this.overlay.toggle(event);
    if (!wasOpen) this.focusMenu();
  }

  public hide(): void {
    this.overlay.hide();
  }

  protected activate(item: UiMenuItem): void {
    // A `routerLink` item navigates off the same native click; here we only run the command and close.
    item.command?.();
    this.hide();
  }

  /**
   * Tabbing past the last item moves focus out of the overlay, and the menu should not linger.
   *
   * `focusout`, not `blur` — blur does not bubble, so a listener on the container would
   * never see focus leaving a child item. The null check matters just as much: clicking dead space
   * inside the menu fires `focusout` with a null `relatedTarget`, and closing on that would make the
   * menu shut whenever the user clicked its own padding.
   */
  protected onFocusOut(event: FocusEvent): void {
    const next = event.relatedTarget as Node | null;
    if (!next || this.overlay.overlayElement?.contains(next)) return;
    this.hide();
  }

  /** Hands focus to the menu container, which arms CdkMenu's FocusKeyManager on the first item. */
  private focusMenu(): void {
    this.overlay.overlayElement
      ?.querySelector<HTMLElement>('[data-slot="dropdown-menu"]')
      ?.focus({ preventScroll: true });
  }
}
