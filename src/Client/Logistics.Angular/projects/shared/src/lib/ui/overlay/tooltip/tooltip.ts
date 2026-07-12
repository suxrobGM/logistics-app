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
import { firstFocusableIn, FOCUSABLE_SELECTOR } from "../../internal/focusable";
import {
  DEFAULT_TOOLTIP_CONTENT_CLASSES,
  DEFAULT_TOOLTIP_SVG_CLASS,
  tooltipPositionVariants,
} from "../../primitives/tooltip";
import { hlm } from "../../primitives/utils";

export type UiTooltipPosition = "top" | "bottom" | "left" | "right";

/**
 * Copy of brain's unexported `BRN_TOOLTIP_POSITIONS_MAP`. These must stay byte-identical to it:
 * `resolveTooltipPosition()` maps a CDK-resolved pair back to a side by comparing against them, and
 * that is what keeps the arrow pointing the right way after a flip.
 */
const TOOLTIP_POSITIONS: Record<UiTooltipPosition, ConnectedPosition> = {
  top: { originX: "center", originY: "top", overlayX: "center", overlayY: "bottom", offsetY: -8 },
  bottom: { originX: "center", originY: "bottom", overlayX: "center", overlayY: "top", offsetY: 8 },
  left: { originX: "start", originY: "center", overlayX: "end", overlayY: "center", offsetX: -8 },
  right: { originX: "end", originY: "center", overlayX: "start", overlayY: "center", offsetX: 8 },
};

/**
 * The tooltip. It reuses brain's `BrnTooltipContent` but owns the trigger and a11y wiring itself,
 * because `BrnTooltip` binds both to its own host — and most hosts here are `<ui-button>`, a wrapper
 * around the real `<button>`. On a wrapper that breaks twice, silently: `focus`/`blur` do not bubble,
 * so brain's focus listener never fires and the tooltip never opens for keyboard users; and
 * `aria-describedby` would land on the non-focusable wrapper, where no screen reader reads it. We
 * listen for `focusin`/`focusout` (which do bubble) and resolve the description onto the focusable
 * descendant. Escape closing is ours too — brain has no keydown handling.
 *
 * A tooltip is a description, never a name: this only writes `aria-describedby` and never touches
 * `aria-label`.
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
    // mouseenter/mouseleave use boundary-crossing semantics: moving between the host and its
    // descendants does not re-fire them. Right for a wrapper.
    "(mouseenter)": "requestShow()",
    "(mouseleave)": "requestHide()",
    // focusin/focusout bubble (focus/blur do not) — this is what catches the inner <button>.
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
  // Must precede the inputs: class fields initialize top-to-bottom and `uiTooltipDelay` reads
  // `config.showDelay` for its default.
  private readonly config = injectBrnTooltipDefaultOptions();

  /**
   * Empty / whitespace / undefined renders no tooltip, never an empty box.
   *
   * A `TemplateRef` is the only supported way to get markup in; an HTML string would be an injection
   * vector, whereas a template's bindings are escaped by Angular.
   */
  public readonly uiTooltip = input<string | TemplateRef<void> | undefined>();
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
    // Keep an open tooltip in sync with text that changes underneath it, and close it if the text
    // goes empty — otherwise a stale or empty box stays on screen.
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

    // The description lands on the focusable element, not on this (possibly wrapper) host.
    this.describedEl = this.resolveDescribedEl();
    this.renderer.setAttribute(this.describedEl, "aria-describedby", this.contentRef.instance.id());

    // On `document`: a hover-opened tooltip leaves focus elsewhere, so a host keydown never sees it.
    const unlistenKey = this.renderer.listen("document", "keydown", (event: KeyboardEvent) => {
      if (event.key === "Escape") this.hide();
    });

    // Close on scroll. Must be capture-phase on `document`: scroll events do not bubble, and this app
    // never scrolls the window — pages scroll inside `div.overflow-y-auto` or the sidebar nav — so a
    // window/bubble-phase listener never fires and a focus-opened tooltip stays stranded away from its
    // host. `Renderer2.listen` cannot pass `capture`, hence the raw listener plus explicit teardown.
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

    // Detach synchronously rather than waiting out an exit animation: keeping the content mounted
    // through a fade-out opens a re-show race that has to be guarded with a generation counter.
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
   * The host if it is focusable itself; else, for a component host that renders its own control
   * (`<ui-button>` -> its inner `<button>`), that control.
   *
   * A native non-focusable host (`<span>`, `<td>`) keeps the description even if it contains a button:
   * a tooltip on a table cell describes the cell, and hoisting it onto a control that merely lives
   * inside would make a screen reader announce the cell's hint as that button's description.
   */
  private resolveDescribedEl(): HTMLElement {
    const host = this.elementRef.nativeElement;
    if (host.matches(FOCUSABLE_SELECTOR)) return host;
    if (!host.tagName.includes("-")) return host; // a native element, not a control wrapper
    return firstFocusableIn(host) ?? host;
  }
}
