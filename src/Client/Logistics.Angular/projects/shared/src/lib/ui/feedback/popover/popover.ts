import type { ConnectedPosition } from "@angular/cdk/overlay";
import { Component, computed, input, viewChild, type TemplateRef } from "@angular/core";
import { AnchoredOverlay } from "../../internal/anchored-overlay";

/** Below the trigger, start-aligned, flipping above it when there is no room. */
const POPOVER_POSITIONS: ConnectedPosition[] = [
  { originX: "start", originY: "bottom", overlayX: "start", overlayY: "top", offsetY: 6 },
  { originX: "start", originY: "top", overlayX: "start", overlayY: "bottom", offsetY: -6 },
  { originX: "end", originY: "bottom", overlayX: "end", overlayY: "top", offsetY: 6 },
  { originX: "end", originY: "top", overlayX: "end", overlayY: "bottom", offsetY: -6 },
];

/**
 * A popover panel with arbitrary projected content. Replaces `<p-popover>` at the 4 TMS chrome sites:
 * the sidebar user menu, the favorites bar, the notification bell and the mobile drawer's user menu.
 *
 * All four drive it imperatively off a template ref — `(click)="userPopover.toggle($event)"` and
 * `(click)="…; userPopover.hide()"` — and the notification bell additionally holds a
 * `viewChild<UiPopover>` and calls `.toggle(event)` from TypeScript. So `toggle(event)` and `hide()`
 * are the contract, and the panel must anchor to whichever button was clicked. That is exactly what
 * `AnchoredOverlay` provides, and it is why this does NOT wrap `HlmPopoverTrigger`: brain's trigger
 * is `button[brnPopoverTrigger]`, it anchors the overlay to its own host, and nothing here has a host
 * to anchor to.
 *
 * The vendored `hlm-popover` primitives are consequently unused by this component — deliberately.
 * Their `*hlmPopoverPortal` structural directive is only load-bearing for brain's own trigger-driven
 * flow; we open and close the CDK overlay ourselves, and every close path is unit-visible in
 * `AnchoredOverlay` rather than implied by a directive that is easy to leave off.
 */
@Component({
  selector: "ui-popover",
  templateUrl: "./popover.html",
})
export class UiPopover {
  /** Panel width, e.g. `"380px"`. The notification bell relied on `[style]="{ width: '380px' }"`. */
  public readonly width = input<string | undefined>(undefined);
  public readonly ariaLabel = input<string | undefined>(undefined);

  private readonly panel = viewChild.required<TemplateRef<unknown>>("panel");

  private readonly overlay = new AnchoredOverlay(() => this.panel(), POPOVER_POSITIONS);

  protected readonly panelStyle = computed(() => (this.width() ? { width: this.width() } : null));

  public isOpen(): boolean {
    return this.overlay.opened();
  }

  public toggle(event: Event): void {
    this.overlay.toggle(event);
  }

  public show(event: Event): void {
    this.overlay.show(event);
  }

  public hide(): void {
    this.overlay.hide();
  }
}
