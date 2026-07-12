import { ESCAPE } from "@angular/cdk/keycodes";
import { Overlay, type ConnectedPosition, type OverlayRef } from "@angular/cdk/overlay";
import { TemplatePortal } from "@angular/cdk/portal";
import { DestroyRef, inject, signal, ViewContainerRef, type TemplateRef } from "@angular/core";
import { firstFocusableIn, isFocusable } from "./focusable";

/**
 * An overlay anchored to whatever element the event came from, with an imperative
 * `toggle(event)` / `hide()`. Internal; shared by `ui-menu` and `ui-popover`.
 *
 * Both are driven through a template ref (`<ui-menu #menu />` plus `menu.toggle($event)` from a
 * *sibling* button), so neither can use the CDK/brain trigger directives: those anchor the overlay to
 * their own host and measure outside-clicks against it. Anchoring to the event's `currentTarget` is
 * the whole point of this class.
 *
 * Close paths, all explicit: Escape (`keydownEvents()`), outside pointer (`outsidePointerEvents()`,
 * ignoring the trigger — see below), navigation (`disposeOnNavigation`) and scrolled-away (the
 * reposition strategy's `autoClose`). `opened` is derived from `detachments()`, so it cannot go stale
 * whichever path fired.
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
    // `currentTarget` is the element the listener is bound to — the trigger. `target` may be an inner
    // <span> or the icon's <svg>, and anchoring to that would hang the panel off the glyph.
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
   * Put focus back on the trigger after Escape — otherwise it falls to <body> and the user's tab
   * position resets.
   *
   * The anchor is usually `<ui-button>`, a wrapper around the real `<button>` (the click listener is
   * bound to the component element, so that is what `currentTarget` hands us). A custom element has no
   * tabindex, so `.focus()` on it is a silent no-op — hence: the anchor only if it is genuinely
   * focusable, else its first focusable child.
   *
   * Takes the anchor as an argument because `hide()` clears `origin` synchronously.
   */
  private static refocusTrigger(anchor: HTMLElement | undefined): void {
    if (!anchor) return;
    const target = isFocusable(anchor) ? anchor : firstFocusableIn(anchor);
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
      // Reposition rather than close: most of these hang off a row in a scrollable table, where
      // closing would make the panel vanish on the smallest trackpad nudge. `autoClose` still detaches
      // once the anchor is scrolled out of view.
      scrollStrategy: this.overlay.scrollStrategies.reposition({ autoClose: true }),
      hasBackdrop: false,
      disposeOnNavigation: true,
    });

    // Drop the anchor on close: one <ui-menu> serves every row of a table, so holding the trigger
    // would pin that row's detached <ui-button> subtree once the table re-renders.
    overlayRef.detachments().subscribe(() => {
      this._opened.set(false);
      this.origin = undefined;
    });

    overlayRef.keydownEvents().subscribe((event) => {
      if (event.keyCode !== ESCAPE) return;
      event.preventDefault();
      const anchor = this.origin; // hide() clears it, synchronously — read it first.
      this.hide();
      AnchoredOverlay.refocusTrigger(anchor);
    });

    overlayRef.outsidePointerEvents().subscribe((event) => {
      // The trigger counts as "outside", and this guard is load-bearing. CDK's outside-click
      // dispatcher listens on <body> in the capture phase, which runs before the trigger's own
      // bubble-phase handler: on a second click of the trigger the order would be [capture]
      // outside-click -> hide, then [bubble] toggle() -> sees a closed panel -> reopens it, so the
      // button could never close what it opened. Ignore the trigger and let `toggle()` own that case.
      const target = event.target as Node | null;
      if (target && this.origin?.contains(target)) return;
      this.hide();
    });

    this.overlayRef = overlayRef;
    return overlayRef;
  }
}

/** Anchored below the trigger and right-aligned, flipping above it when there is no room. */
export const BELOW_TRIGGER_POSITIONS: ConnectedPosition[] = [
  { originX: "end", originY: "bottom", overlayX: "end", overlayY: "top", offsetY: 4 },
  { originX: "end", originY: "top", overlayX: "end", overlayY: "bottom", offsetY: -4 },
  { originX: "start", originY: "bottom", overlayX: "start", overlayY: "top", offsetY: 4 },
  { originX: "start", originY: "top", overlayX: "start", overlayY: "bottom", offsetY: -4 },
];
