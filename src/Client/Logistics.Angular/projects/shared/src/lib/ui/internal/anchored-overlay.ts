import { ESCAPE } from "@angular/cdk/keycodes";
import { Overlay, type ConnectedPosition, type OverlayRef } from "@angular/cdk/overlay";
import { TemplatePortal } from "@angular/cdk/portal";
import { DestroyRef, inject, signal, ViewContainerRef, type TemplateRef } from "@angular/core";

/**
 * An overlay anchored to WHATEVER ELEMENT THE EVENT CAME FROM, with an imperative
 * `toggle(event)` / `hide()`.
 *
 * INTERNAL. Not exported from `@logistics/shared/ui` — it exists only so `ui-menu` and `ui-popover`
 * share ONE implementation of "open here, and definitely close again". Both replace a PrimeNG
 * component whose call sites drive it through a template ref (`<ui-menu #menu />` plus
 * `menu.toggle($event)` from a *sibling* button), so neither can use the CDK/brain trigger
 * directives: those anchor their overlay to their OWN host element, and measure outside-clicks
 * against it. Anchoring to the event's `currentTarget` is the whole point of this class.
 *
 * Every close path is explicit and lives here, once (Lesson 5 — "the overlay must CLOSE", and this
 * migration has already shipped that bug):
 *
 *   - Escape             `keydownEvents()`
 *   - Outside pointer    `outsidePointerEvents()`, ignoring the trigger itself — see below
 *   - Navigation         `disposeOnNavigation`
 *   - Scrolled away      the reposition strategy's `autoClose`
 *
 * and `opened` is derived from `detachments()`, so it cannot go stale no matter which path fired.
 * Callers layer their own extra closers on top (an activated menu item; a `hide()` from a link).
 */
export class AnchoredOverlay {
  private readonly overlay = inject(Overlay);
  private readonly viewContainerRef = inject(ViewContainerRef);

  private readonly _opened = signal(false);
  /** Read-only view of whether the panel is currently attached. */
  public readonly opened = this._opened.asReadonly();

  private overlayRef?: OverlayRef;
  private origin?: HTMLElement;

  /**
   * @param panel     the `<ng-template>` holding the panel content
   * @param positions CDK anchors, best first
   */
  constructor(
    private readonly panel: () => TemplateRef<unknown>,
    private readonly positions: ConnectedPosition[],
  ) {
    inject(DestroyRef).onDestroy(() => this.overlayRef?.dispose());
  }

  /** The element the panel is currently anchored to (the button that opened it). */
  public get anchor(): HTMLElement | undefined {
    return this.origin;
  }

  public get overlayElement(): HTMLElement | undefined {
    return this.overlayRef?.overlayElement;
  }

  public toggle(event: Event): void {
    if (this._opened()) {
      this.hide();
    } else {
      this.show(event);
    }
  }

  public show(event: Event): void {
    // `currentTarget` is the element the listener is bound to — the trigger button. `target` may be
    // an inner <span> or the icon's <svg>, and anchoring to that would hang the panel off the glyph.
    const origin = (event.currentTarget ?? event.target) as HTMLElement | null;
    if (!origin) return;

    this.origin = origin;
    this.ensureOverlay(origin).attach(new TemplatePortal(this.panel(), this.viewContainerRef));
    this._opened.set(true);
  }

  public hide(): void {
    this.overlayRef?.detach();
  }

  /**
   * Put focus back where it came from after Escape — otherwise it falls to <body> and the user's tab
   * position resets, which PrimeNG did not do.
   *
   * The anchor is usually `<ui-button>`, a WRAPPER around the real `<button>` (the click listener is
   * bound to the component element, so that is what `event.currentTarget` hands us). A custom element
   * has no tabindex, so `anchor.focus()` is a silent no-op — the same wrapper trap that broke keyboard
   * tooltips in S6. Focus the anchor only if it is genuinely focusable, else its first focusable child.
   */
  private refocusTrigger(): void {
    const anchor = this.origin;
    if (!anchor) return;
    const target = anchor.tabIndex >= 0 ? anchor : anchor.querySelector<HTMLElement>(FOCUSABLE);
    target?.focus({ preventScroll: true });
  }

  private ensureOverlay(origin: HTMLElement): OverlayRef {
    const positionStrategy = this.overlay
      .position()
      .flexibleConnectedTo(origin)
      .withPush(true)
      .withPositions(this.positions);

    if (this.overlayRef) {
      // One <ui-menu> serves every row of a table, so the anchor changes on each open.
      this.overlayRef.updatePositionStrategy(positionStrategy);
      return this.overlayRef;
    }

    const overlayRef = this.overlay.create({
      positionStrategy,
      // Reposition rather than close: most of these hang off a row inside a scrollable table, and
      // closing would make the panel vanish on the smallest trackpad nudge. `autoClose` still
      // detaches once the anchor is scrolled out of view, which is the case where staying glued looks
      // broken.
      scrollStrategy: this.overlay.scrollStrategies.reposition({ autoClose: true }),
      hasBackdrop: false,
      disposeOnNavigation: true,
    });

    overlayRef.detachments().subscribe(() => this._opened.set(false));

    overlayRef.keydownEvents().subscribe((event) => {
      if (event.keyCode !== ESCAPE) return;
      event.preventDefault();
      this.hide();
      this.refocusTrigger();
    });

    overlayRef.outsidePointerEvents().subscribe((event) => {
      // THE TRIGGER COUNTS AS "OUTSIDE", AND THIS GUARD IS LOAD-BEARING.
      // CDK's OverlayOutsideClickDispatcher listens on <body> in the CAPTURE phase and emits on the
      // `click`. Capture runs before the trigger's own bubble-phase handler, so on a SECOND click of
      // the kebab the order is: [capture] outside-click -> hide, then [bubble] toggle() -> sees a
      // closed panel -> reopens it. The button would be unable to close what it opened, and the menu
      // would look stuck. So ignore the trigger here and let `toggle()` own that case.
      const target = event.target as Node | null;
      if (target && this.origin?.contains(target)) return;
      this.hide();
    });

    this.overlayRef = overlayRef;
    return overlayRef;
  }
}

/** What actually takes focus inside a wrapper element like `<ui-button>`. */
const FOCUSABLE = 'button, a[href], input, select, textarea, [tabindex]:not([tabindex="-1"])';

/** Anchored below the trigger and right-aligned, flipping above it when there is no room. */
export const BELOW_TRIGGER_POSITIONS: ConnectedPosition[] = [
  { originX: "end", originY: "bottom", overlayX: "end", overlayY: "top", offsetY: 4 },
  { originX: "end", originY: "top", overlayX: "end", overlayY: "bottom", offsetY: -4 },
  { originX: "start", originY: "bottom", overlayX: "start", overlayY: "top", offsetY: 4 },
  { originX: "start", originY: "top", overlayX: "start", overlayY: "bottom", offsetY: -4 },
];
