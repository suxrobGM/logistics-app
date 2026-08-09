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
 * The popup menu. `CdkMenu` supplies arrow keys, type-ahead and Home/End; this class supplies the
 * overlay and every close path.
 *
 * ```html
 * <ui-button icon="ellipsis-vertical" (click)="selectedRow.set(row); menu.toggle($event)" />
 * <ui-menu #menu [items]="actionMenuItems()" />
 * ```
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
   * `focusout`, not `blur` - blur does not bubble, so a listener on the container would
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
