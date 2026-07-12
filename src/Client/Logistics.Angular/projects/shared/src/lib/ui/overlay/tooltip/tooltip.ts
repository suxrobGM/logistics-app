import {
  Overlay,
  OverlayPositionBuilder,
  type ConnectedPosition,
  type FlexibleConnectedPositionStrategy,
  type OverlayRef,
} from "@angular/cdk/overlay";
import { ComponentPortal } from "@angular/cdk/portal";
import {
  computed,
  DestroyRef,
  Directive,
  DOCUMENT,
  effect,
  ElementRef,
  inject,
  input,
  numberAttribute,
  Renderer2,
  type ComponentRef,
  type TemplateRef,
} from "@angular/core";
import {
  BRN_TOOLTIP_FALLBACK_POSITIONS,
  BrnTooltipContent,
  injectBrnTooltipDefaultOptions,
  provideBrnTooltipDefaultOptions,
  resolveTooltipPosition,
  type BrnTooltipType,
} from "@spartan-ng/brain/tooltip";
import type { Subscription } from "rxjs";
import {
  DEFAULT_TOOLTIP_CONTENT_CLASSES,
  DEFAULT_TOOLTIP_SVG_CLASS,
  tooltipPositionVariants,
} from "../../primitives/tooltip";
import { hlm } from "../../primitives/utils";

export type UiTooltipPosition = "top" | "bottom" | "left" | "right";

/**
 * CDK anchors, byte-identical to `BRN_TOOLTIP_POSITIONS_MAP` (which brain does not export).
 * They MUST stay identical: `resolveTooltipPosition()` maps a CDK-resolved pair back to a side by
 * comparing these four anchors, and it is what keeps the arrow pointing the right way after a flip.
 */
const TOOLTIP_POSITIONS: Record<UiTooltipPosition, ConnectedPosition> = {
  top: { originX: "center", originY: "top", overlayX: "center", overlayY: "bottom", offsetY: -8 },
  bottom: { originX: "center", originY: "bottom", overlayX: "center", overlayY: "top", offsetY: 8 },
  left: { originX: "start", originY: "center", overlayX: "end", overlayY: "center", offsetX: -8 },
  right: { originX: "end", originY: "center", overlayX: "start", overlayY: "center", offsetX: 8 },
};

/** What a screen reader will actually focus — i.e. what `aria-describedby` has to land on. */
const FOCUSABLE = 'button, a[href], input, select, textarea, [tabindex]:not([tabindex="-1"])';

/**
 * The tooltip. Replaces `pTooltip` at 124 call sites.
 *
 * WHY THIS OWNS ITS OVERLAY INSTEAD OF WRAPPING `BrnTooltip`
 * `BrnTooltip` is already a one-attribute directive (`HlmTooltip` is a thin `hostDirectives` alias of
 * it), so wrapping it looked like the whole job. It is not, because `BrnTooltip` binds its triggers
 * and its `aria-describedby` to ITS OWN HOST ELEMENT — and 82 of our 124 hosts are `<ui-button>`,
 * which is a WRAPPER around the real `<button>` (see `UiButton`: it keeps the wrapper node on
 * purpose). Two things break on a wrapper, both silently:
 *
 *   1. `focus`/`blur` DO NOT BUBBLE. brain listens for `focus` on the wrapper, the inner `<button>`
 *      is what actually receives focus, so the listener never fires and the tooltip NEVER OPENS ON
 *      KEYBOARD FOCUS. Nothing throws; it just doesn't work for keyboard users.
 *   2. `aria-describedby` would be written onto `<ui-button>`, which is not the focusable node, so a
 *      screen reader parked on the inner `<button>` never announces the description.
 *
 * So we keep brain's *content* (`BrnTooltipContent` — styled, animated, `role="tooltip"`, with the
 * arrow and the `data-side`/`data-state` hooks) and Helm's styling, and own only the trigger and
 * a11y wiring, which is the part that has to know about the wrapper. `focusin`/`focusout` DO bubble,
 * so they catch the inner button's focus from the host; `aria-describedby` is resolved onto the
 * focusable descendant.
 *
 * ESCAPE
 * brain has no keydown handling at all. Users expect Escape to dismiss an open tooltip, so
 * dropping that support would have been a silent a11y regression.
 *
 * ACCESSIBLE NAME vs DESCRIPTION
 * A tooltip is a DESCRIPTION, never a name. This directive only ever writes `aria-describedby`; it
 * does not touch `aria-label`, so the explicit `ariaLabel` on every icon-only button survives.
 *
 * @example
 * <ui-button icon="trash" ariaLabel="Delete load" uiTooltip="Delete load" />
 * <ui-button icon="refresh-cw" ariaLabel="Refresh" uiTooltip="Refresh" uiTooltipPosition="bottom" />
 */
@Directive({
  selector: "[uiTooltip]",
  exportAs: "uiTooltip",
  providers: [
    provideBrnTooltipDefaultOptions({
      svgClasses: DEFAULT_TOOLTIP_SVG_CLASS,
      tooltipContentClasses: DEFAULT_TOOLTIP_CONTENT_CLASSES,
      arrowClasses: (position) => hlm(tooltipPositionVariants({ position })),
    }),
  ],
  host: {
    // mouseenter/mouseleave use boundary-crossing semantics: entering a descendant does not re-fire
    // on the host, and leaving to a descendant does not fire mouseleave. Exactly right for a wrapper.
    "(mouseenter)": "requestShow()",
    "(mouseleave)": "requestHide()",
    // focusin/focusout DO bubble (focus/blur do not) — this is what makes the inner <button> work.
    "(focusin)": "requestShow()",
    "(focusout)": "requestHide()",
  },
})
export class UiTooltip {
  private readonly elementRef = inject<ElementRef<HTMLElement>>(ElementRef);
  private readonly renderer = inject(Renderer2);
  private readonly overlay = inject(Overlay);
  private readonly positionBuilder = inject(OverlayPositionBuilder);
  private readonly document = inject(DOCUMENT);
  // Declared before the inputs on purpose: class fields initialize top-to-bottom, and
  // `uiTooltipDelay` reads `config.showDelay` for its default.
  private readonly config = injectBrnTooltipDefaultOptions();

  /**
   * Empty / whitespace / undefined renders NO tooltip — never an empty box.
   *
   * A `TemplateRef` is accepted for the rare tooltip that needs structure. It is the ONLY supported
   * way to get markup in: a raw `innerHTML` string was interpolating tenant address data straight
   * into itself at the one call site that needed structure. A template's bindings are escaped by
   * Angular, so the markup stays and the injection does not.
   */
  public readonly uiTooltip = input<string | TemplateRef<void> | undefined>();
  /** `"right"` is the default; the 79 call sites that set no position rely on it. */
  public readonly uiTooltipPosition = input<UiTooltipPosition>("right");
  public readonly uiTooltipDelay = input(this.config.showDelay, { transform: numberAttribute });

  /** `null` when there is nothing to show. Whitespace-only is nothing. */
  private readonly content = computed<BrnTooltipType>(() => {
    const value = this.uiTooltip();
    if (typeof value === "string") return value.trim() || null;
    return value ?? null;
  });

  private overlayRef?: OverlayRef;
  private contentRef?: ComponentRef<BrnTooltipContent>;
  private positionSub?: Subscription;
  private showTimer?: ReturnType<typeof setTimeout>;
  private describedEl?: HTMLElement;
  private teardownGlobals?: () => void;
  private activePosition?: UiTooltipPosition;

  constructor() {
    // Keep an open tooltip in sync with a text that changes underneath it, and close it outright if
    // the text goes empty — otherwise we would be left showing a stale or empty box.
    effect(() => {
      const content = this.content();
      if (!this.contentRef) return;
      if (!content) {
        this.hide();
        return;
      }
      this.applyProps(this.activePosition ?? this.uiTooltipPosition());
    });

    inject(DestroyRef).onDestroy(() => {
      this.hide();
      this.overlayRef?.dispose();
      this.overlayRef = undefined;
    });
  }

  protected requestShow(): void {
    if (!this.content() || this.contentRef || this.showTimer) return;
    this.showTimer = setTimeout(() => {
      this.showTimer = undefined;
      this.show();
    }, this.uiTooltipDelay());
  }

  protected requestHide(): void {
    this.hide();
  }

  private show(): void {
    if (!this.content() || this.contentRef) return;

    const overlayRef = this.ensureOverlay();
    this.contentRef = overlayRef.attach(new ComponentPortal(BrnTooltipContent));
    this.contentRef.instance.state.set("open");
    this.applyProps(this.uiTooltipPosition());

    // Keep the arrow and `data-side` in sync when the CDK flips us to a fallback position.
    const strategy = overlayRef.getConfig().positionStrategy as FlexibleConnectedPositionStrategy;
    this.positionSub = strategy.positionChanges.subscribe((change) => {
      const resolved = resolveTooltipPosition(change.connectionPair);
      if (resolved && this.contentRef) this.applyProps(resolved);
    });

    // The description lands on the FOCUSABLE element, not on this (possibly wrapper) host.
    this.describedEl = this.resolveDescribedEl();
    this.renderer.setAttribute(this.describedEl, "aria-describedby", this.contentRef.instance.id());

    // Escape closes even when the tooltip was opened by hover (focus is then elsewhere, so a host
    // keydown would never see it).
    const unlistenKey = this.renderer.listen("document", "keydown", (event: KeyboardEvent) => {
      if (event.key === "Escape") this.hide();
    });

    // Scroll closes, matching brain. This MUST be a CAPTURE-phase listener on `document`:
    // scroll events DO NOT BUBBLE, and this app never scrolls the window — every page scrolls inside
    // `div.overflow-y-auto` (the main content pane) or the sidebar nav. A `window`- or bubble-phase
    // listener therefore NEVER FIRES here, which left a focus-opened tooltip stranded hundreds of px
    // from its host once the user scrolled (the hover path hid it by accident, via the `mouseleave`
    // that firing from the host scrolling out from under the cursor — so only KEYBOARD users saw it).
    // Capture-phase fires for a scroll on any element, so it catches those containers.
    // `Renderer2.listen` cannot pass `capture`, hence the raw listener plus explicit teardown.
    const onScroll = () => this.hide();
    this.document.addEventListener("scroll", onScroll, { capture: true, passive: true });

    this.teardownGlobals = () => {
      unlistenKey();
      this.document.removeEventListener("scroll", onScroll, { capture: true });
    };
  }

  private hide(): void {
    if (this.showTimer) {
      clearTimeout(this.showTimer);
      this.showTimer = undefined;
    }
    if (!this.contentRef) return;

    // Detach synchronously rather than waiting out the exit animation. brain keeps the content
    // mounted through its fade-out and guards the re-show race with a generation counter; we drop
    // the fade-out instead of importing that race. A tooltip that reliably closes beats a prettier
    // one that can get stuck open.
    this.teardownGlobals?.();
    this.teardownGlobals = undefined;
    this.positionSub?.unsubscribe();
    this.positionSub = undefined;

    if (this.describedEl) {
      this.renderer.removeAttribute(this.describedEl, "aria-describedby");
      this.describedEl = undefined;
    }

    this.overlayRef?.detach();
    this.contentRef = undefined;
    this.activePosition = undefined;
  }

  private ensureOverlay(): OverlayRef {
    const positions = [
      this.uiTooltipPosition(),
      ...BRN_TOOLTIP_FALLBACK_POSITIONS[this.uiTooltipPosition()],
    ].map((side) => TOOLTIP_POSITIONS[side]);

    if (!this.overlayRef) {
      this.overlayRef = this.overlay.create({
        positionStrategy: this.positionBuilder
          .flexibleConnectedTo(this.elementRef)
          .withPositions(positions)
          .withViewportMargin(8),
        // We close on scroll ourselves; a CDK strategy that detached behind our back would strand
        // `contentRef` and leave a dangling aria-describedby.
        scrollStrategy: this.overlay.scrollStrategies.noop(),
      });
    } else {
      (
        this.overlayRef.getConfig().positionStrategy as FlexibleConnectedPositionStrategy
      ).withPositions(positions);
    }
    return this.overlayRef;
  }

  private applyProps(position: UiTooltipPosition): void {
    this.activePosition = position;
    this.contentRef?.instance.setProps(
      this.content(),
      position,
      this.config.tooltipContentClasses,
      this.config.arrowClasses(position),
      this.config.svgClasses,
    );
  }

  /**
   * The host when it is itself focusable (`<button uiTooltip>`, `<a uiTooltip>`); otherwise, for a
   * COMPONENT host that renders its own control (`<ui-button>` -> its inner `<button>`), that control.
   *
   * A native non-focusable host — `<span>`, `<td>`, `<label>` — keeps the description on ITSELF even
   * if it happens to contain a button. The tooltip on a table cell describes the cell; hoisting it
   * onto some control that merely lives inside the cell would make a screen reader announce the
   * cell's hint as that button's description, which is worse than announcing nothing.
   */
  private resolveDescribedEl(): HTMLElement {
    const host = this.elementRef.nativeElement;
    if (host.matches(FOCUSABLE)) return host;
    if (!host.tagName.includes("-")) return host; // a native element, not a control wrapper
    return host.querySelector<HTMLElement>(FOCUSABLE) ?? host;
  }
}
