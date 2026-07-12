import { Component, computed, input, model } from "@angular/core";
import { HlmSheet, HlmSheetContent, HlmSheetPortal, HlmSheetTitle } from "../../primitives/sheet";

export type UiDrawerPosition = "left" | "right" | "top" | "bottom";

/**
 * A side panel. Replaces `<p-drawer>` at 3 call sites: the TMS and admin mobile navigation drawers
 * (`position="left"`) and the TMS conversation-details panel (`position="right"`).
 *
 * SHEET, NOT DRAWER — the two Helm primitives with those names are not interchangeable.
 * `hlm-drawer` is the vaul-style one: it is built for a *draggable* bottom sheet, ships a drag handle,
 * takes a `closeThreshold`, and positions itself off `data-vaul-drawer-direction`. `hlm-sheet` is a
 * plain edge-anchored panel on `BrnDialog`, with a `side` input and a global position strategy. All
 * three of our call sites are plain edge panels with no drag affordance, so `hlm-sheet` is the honest
 * match; `hlm-drawer` would have added a grab handle nobody asked for.
 *
 * `*hlmSheetPortal` in the template is LOAD-BEARING: it portals the content into the overlay, and
 * without it the sheet never opens and never closes — and nothing throws.
 *
 * The `[modal]`, `[dismissible]` and `[closeOnEscape]` inputs are gone because `BrnDialog`
 * already does all three by default: `hasBackdrop: true`, backdrop-click dismisses, and Escape
 * dismisses unless `disableClose` is set. Re-exposing them as inputs would only have created a way to
 * turn them off by accident.
 */
@Component({
  selector: "ui-drawer",
  templateUrl: "./drawer.html",
  imports: [HlmSheet, HlmSheetContent, HlmSheetPortal, HlmSheetTitle],
})
export class UiDrawer {
  /** `model`, so both `[(visible)]` and `[visible]` + `(visibleChange)` bind — the call sites use both. */
  public readonly visible = model(false);

  public readonly position = input<UiDrawerPosition>("left");
  /** e.g. `"300px"`. Clamped to 85vw so a wide drawer cannot swallow a small viewport. */
  public readonly width = input<string | undefined>(undefined);
  public readonly showCloseIcon = input(true);
  /** A plain-text title. For custom header markup, project an element with `uiDrawerHeader` instead. */
  public readonly header = input<string | undefined>(undefined);

  protected readonly state = computed(() => (this.visible() ? "open" : "closed"));

  protected readonly contentStyle = computed(() =>
    this.width() ? { width: this.width(), maxWidth: "85vw" } : null,
  );

  /** Brain drives dismissal itself (backdrop, Escape, close button); mirror it back into `visible`. */
  protected onStateChanged(next: "open" | "closed"): void {
    this.visible.set(next === "open");
  }
}
