import { CdkDrag, CdkDragHandle } from "@angular/cdk/drag-drop";
import { DOCUMENT, NgTemplateOutlet } from "@angular/common";
import {
  booleanAttribute,
  ChangeDetectionStrategy,
  Component,
  contentChild,
  effect,
  inject,
  input,
  model,
  output,
  signal,
  TemplateRef,
} from "@angular/core";
import type { BrnDialogState } from "@spartan-ng/brain/dialog";
import { isTopmostOverlay } from "../../internal/overlay-stack";
import {
  HlmDialog,
  HlmDialogContent,
  HlmDialogFooter,
  HlmDialogHeader,
  HlmDialogPortal,
} from "../../primitives/dialog";

/** Per-instance ids: the breakpoint stylesheet and `aria-labelledby` both need a stable handle. */
let nextDialogId = 0;

/** Floor for the resize grip, so a dialog can never be dragged down to nothing. */
const MIN_RESIZE_WIDTH = 240;
const MIN_RESIZE_HEIGHT = 140;

/**
 * The modal dialog. Replaces `<p-dialog>` at 46 call sites / 42 files.
 *
 * =================================================================================================
 * `descendants: false` IS LOAD-BEARING. Do not drop it "because the default is fine".
 * =================================================================================================
 * p-dialog declares all three of its projection slots EXPLICITLY non-descending (primeng-dialog.mjs):
 *
 *     @ContentChild('header',  { descendants: false }) _headerTemplate;
 *     @ContentChild('content', { descendants: false }) _contentTemplate;
 *     @ContentChild('footer',  { descendants: false }) _footerTemplate;
 *
 * Angular's `contentChild()` defaults `descendants` to TRUE. Those are not the same query, and in
 * this repo the difference is not hypothetical — it is a live bug waiting to happen:
 *
 *     <p-dialog header="Edit Customer">          ← customer-edit-dialog.html
 *       <form>…</form>
 *       <p-accordion>
 *         <p-accordion-panel value="danger">
 *           <ng-template #header>  Danger Zone   ← the ACCORDION PANEL's header slot
 *           <ng-template #content> Delete Customer…
 *
 * `#header` and `#content` are descendants of the dialog in that template, so a DESCENDING query
 * matches them. The dialog would then render "Danger Zone" as its own title and the delete-customer
 * panel as its own body — while the accordion, robbed of its templates, renders empty. Same shape in
 * employee-edit-dialog, and again wherever a `ui-data-table` (whose own slots are also `#header` /
 * `#footer`) is nested inside a dialog: payment-link, tracking-link, attach-load.
 *
 * Every `#header` / `#content` written inside a `<p-dialog>` in this repo TODAY belongs to a nested
 * component, not to the dialog. `descendants: false` is the only thing that tells them apart, and it
 * is exactly the bug S5 shipped when `ui-card` stole nested tables' `#header` rows.
 *
 * Declaring the same query with the same flag also settles the `@if` case by construction:
 * driver-behavior-list writes its `<ng-template #footer>` inside an `@if (selectedEvent())`. Whatever
 * Angular's matching rules are there, they are IDENTICAL for p-dialog and for us, because it is the
 * same query primitive with the same flag. Behaviour is preserved without having to reason about it.
 *
 * =================================================================================================
 * WHY THE BACKDROP MUST NOT CLOSE THE DIALOG (this would have been a silent data-loss regression)
 * =================================================================================================
 * p-dialog only closes on a mask click when `dismissableMask` is set (primeng-dialog.mjs:635):
 *
 *     if (this.closable && this.dismissableMask) { …bind mousedown on the mask… }
 *
 * `dismissableMask` defaults to FALSE and NOT ONE of the 46 call sites sets it. So no dialog in this
 * app closes on a backdrop click today — you cannot lose a half-filled form by clicking beside it.
 *
 * brain's default is the opposite (spartan-ng-brain-dialog.mjs:94):
 *
 *     dismiss(reason) {
 *       if (!this.open || options.disableClose) return false;   // gates BOTH escape and backdrop
 *       if (reason === 'outside' && !options.closeOnOutsidePointerEvents) return false;
 *       this.close(); return true;                              // 'backdrop' reaches here ALWAYS
 *     }
 *
 * `'backdrop'` is never gated by `closeOnOutsidePointerEvents` — only `'outside'` is — so with brain's
 * defaults every one of the 46 dialogs would have silently acquired click-outside-to-discard.
 *
 * There is no option combination meaning "escape closes, the backdrop does not", so — exactly as
 * `ui-confirm-dialog` already does — we pass `disableClose: true` (brain never closes the dialog on
 * its own) and wire Escape ourselves below.
 *
 * That is also the only correct choice for a REACTIVE `closable`: brain snapshots its options into
 * `initialOptions` at open() time and `dismiss()` reads that snapshot, so mapping `closable` onto
 * `disableClose` would freeze it at whatever it was when the dialog opened —
 * `confirm-delete-dialog`'s `[closable]="!deleting()"` would never take effect.
 *
 * =================================================================================================
 * WIDTH LANDS ON THE PORTALLED PANEL, NOT ON THE HOST
 * =================================================================================================
 * CDK portals the content out of this component's view, so a width on `<ui-dialog>` itself would be a
 * no-op and all 46 dialogs would render at Helm's default `sm:max-w-md` (28rem) — a 650px dialog
 * silently clamped to 448px. The width therefore goes as an inline style on `hlm-dialog-content`, the
 * element that actually renders the panel inside the overlay, where it also beats Helm's `sm:max-w-md`
 * and `max-w-[calc(100%-2rem)]` classes (inline styles win over classes).
 *
 * `maxWidth` defaults to `calc(100vw - 2rem)` whenever an explicit width is set, so overriding Helm's
 * max-width does not let a 650px dialog run off the side of a phone.
 */
@Component({
  selector: "ui-dialog",
  templateUrl: "./dialog.html",
  imports: [
    HlmDialog,
    HlmDialogPortal,
    HlmDialogContent,
    HlmDialogHeader,
    HlmDialogFooter,
    NgTemplateOutlet,
    CdkDrag,
    CdkDragHandle,
  ],
  changeDetection: ChangeDetectionStrategy.OnPush,
  host: {
    // On `document`, not on the host: the panel is portalled into the CDK overlay container, which is
    // NOT inside this component's DOM, so a host-scoped keydown would never see the event. (This also
    // matches p-dialog, which binds its escape listener on the document — primeng-dialog.mjs:890.)
    "(document:keydown.escape)": "onEscape()",
  },
})
export class UiDialog {
  private readonly document = inject(DOCUMENT);

  /** Distinguishes this instance's panel for the `breakpoints` stylesheet and the a11y label. */
  private readonly uid = `ui-dialog-${nextDialogId++}`;
  protected readonly panelId = `${this.uid}-panel`;
  protected readonly titleId = `${this.uid}-title`;

  /**
   * p-dialog's `[(visible)]`. A `model` covers both call-site shapes: the 43 banana-boxes and the 3
   * that split it into `[visible]` + `(visibleChange)` (or, in upgrade-dialog, a one-way `[visible]`
   * that relies on `(onHide)` to push the state back).
   */
  public readonly open = model(false);

  /** The title. Static at 41 sites, bound/interpolated at 5. */
  public readonly header = input<string>("");

  /** e.g. `450px`, `32rem`, `80vw`. Unset -> Helm's default panel width. */
  public readonly width = input<string>();

  /** Only 2 call sites set it (both `1200px`, paired with a vw width). */
  public readonly maxWidth = input<string>();

  /** p-dialog's `[breakpoints]`: `{ '640px': '95vw' }` — max-width media queries. 1 call site. */
  public readonly breakpoints = input<Record<string, string>>();

  /** Shows the X, and gates Escape. p-dialog: `closable` defaults true and gates both. */
  public readonly closable = input(true, { transform: booleanAttribute });

  /**
   * p-dialog's `draggable` DEFAULTS TO TRUE (primeng-dialog.mjs:151), and all 10 call sites that
   * mention it pass `false` — they are opting OUT. So 36 of the 46 dialogs are draggable today and
   * the default here must stay `true` or we would silently remove the affordance from all of them.
   */
  public readonly draggable = input(true, { transform: booleanAttribute });

  /** Same story: p-dialog's `resizable` defaults TRUE (:156); 2 sites opt out, 1 redundantly opts in. */
  public readonly resizable = input(true, { transform: booleanAttribute });

  /** Every call site passes `true`; kept so the tag stays a faithful drop-in. Drives the backdrop. */
  public readonly modal = input(true, { transform: booleanAttribute });

  /**
   * p-dialog's `(onShow)` (11 sites) and `(onHide)` (16 sites) — and they are NOT the same event.
   * brain exposes a single `stateChanged` that fires on BOTH transitions; collapsing the two onto it
   * would make every `(onHide)` handler run on OPEN as well, and most of them reset or clear the form
   * — prefilled edit dialogs would blank themselves the moment they appeared. So they stay separate.
   */
  public readonly opened = output<void>();
  public readonly closed = output<void>();

  /* See the class comment: `descendants: false` is load-bearing, not a stylistic choice. */
  protected readonly headerTpl = contentChild<TemplateRef<unknown>>("header", {
    descendants: false,
  });
  protected readonly contentTpl = contentChild<TemplateRef<unknown>>("content", {
    descendants: false,
  });
  protected readonly footerTpl = contentChild<TemplateRef<unknown>>("footer", {
    descendants: false,
  });

  /** Live size while the grip is dragged; overrides `width` until the dialog is closed. */
  private readonly resizedWidth = signal<number | null>(null);
  private readonly resizedHeight = signal<number | null>(null);

  protected readonly panelWidth = () => {
    const resized = this.resizedWidth();
    return resized === null ? this.width() : `${resized}px`;
  };

  protected readonly panelHeight = () => {
    const resized = this.resizedHeight();
    return resized === null ? undefined : `${resized}px`;
  };

  /**
   * Helm caps the panel at `sm:max-w-md`; an inline width has to beat that, which means also replacing
   * the max-width — otherwise a 650px dialog is clamped to 448px. Replacing it with a viewport-relative
   * cap keeps the "never wider than the screen" guarantee that `max-w-[calc(100%-2rem)]` gave us.
   */
  protected readonly panelMaxWidth = () =>
    this.maxWidth() ?? (this.width() ? "calc(100vw - 2rem)" : undefined);

  protected readonly state = () => (this.open() ? "open" : "closed") satisfies BrnDialogState;

  constructor() {
    // p-dialog compiles `breakpoints` into a media-query stylesheet; we do the same, scoped by the
    // panel's id. It has to be a real stylesheet: a media query cannot be expressed as an inline
    // style, and the rule must out-rank the inline width — hence `!important`.
    effect((onCleanup) => {
      const breakpoints = this.breakpoints();
      if (!breakpoints) return;

      const style = this.document.createElement("style");
      style.textContent = Object.entries(breakpoints)
        .map(
          ([maxWidth, width]) =>
            `@media screen and (max-width: ${maxWidth}) {` +
            `#${this.panelId} { width: ${width} !important; max-width: ${width} !important; }` +
            `}`,
        )
        .join("\n");
      this.document.head.appendChild(style);
      onCleanup(() => style.remove());
    });
  }

  /** brain fires this on both transitions; split it back into the two events the call sites expect. */
  protected onStateChanged(state: BrnDialogState): void {
    if (state === "open") {
      this.opened.emit();
      return;
    }

    // A close can originate inside the overlay (the X, Escape) as well as from the caller flipping
    // `open`, so push the state back out before announcing it — that is what makes `[(open)]` work.
    this.resizedWidth.set(null);
    this.resizedHeight.set(null);
    this.open.set(false);
    this.closed.emit();
  }

  /**
   * p-dialog closes on Escape when `closeOnEscape && closable` (primeng-dialog.mjs:838). No call site
   * touches `closeOnEscape` (default true), so `closable` alone decides — and it is read live here,
   * which is the point of not delegating to brain's open-time `disableClose` snapshot.
   *
   * `isTopmostOverlay` is NOT optional: a dropdown opened inside this dialog is a separate CDK overlay
   * stacked above it, and its Escape must not reach us. PrimeNG's select stopped the event itself;
   * Helm's does not, so without this guard one Escape dismissed the dropdown AND discarded the form
   * underneath it. See `internal/overlay-stack.ts` — this cost a real bug.
   */
  protected onEscape(): void {
    if (!this.open() || !this.closable()) return;
    if (!isTopmostOverlay(this.document.getElementById(this.panelId))) return;

    this.open.set(false);
  }

  protected startResize(event: PointerEvent): void {
    // Without this the browser's own drag/selection kicks in and the grip stutters.
    event.preventDefault();

    const panel = this.document.getElementById(this.panelId);
    if (!panel) return;

    const { width, height } = panel.getBoundingClientRect();
    const startX = event.clientX;
    const startY = event.clientY;

    // The overlay CENTRES the panel, so it grows from BOTH edges: the right edge only advances by half
    // of whatever we add to the width. Doubling the delta is what keeps the grip under the pointer.
    const onMove = (move: PointerEvent) => {
      this.resizedWidth.set(Math.max(MIN_RESIZE_WIDTH, width + (move.clientX - startX) * 2));
      this.resizedHeight.set(Math.max(MIN_RESIZE_HEIGHT, height + (move.clientY - startY) * 2));
    };
    const stop = () => {
      this.document.removeEventListener("pointermove", onMove);
      this.document.removeEventListener("pointerup", stop);
      this.document.removeEventListener("pointercancel", stop);
    };

    // Listen on the DOCUMENT, not on the grip, and do NOT rely on pointer capture.
    //
    // `setPointerCapture()` THROWS (NotFoundError: "No active pointer with the given id") whenever the
    // pointerId is not a live pointer. Calling it before wiring the listeners — which is what this
    // originally did — meant that one throw aborted `startResize` before a single listener was
    // attached, and the grip silently did nothing at all. Capture is an optimisation; correctness must
    // not hang off it. Document-level listeners already keep receiving the move after the pointer
    // leaves the 16px grip, which is the only thing capture was buying us, and `pointercancel` covers
    // the pointer being torn out from under us.
    this.document.addEventListener("pointermove", onMove);
    this.document.addEventListener("pointerup", stop);
    this.document.addEventListener("pointercancel", stop);
  }
}
