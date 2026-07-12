import { CdkDrag, CdkDragHandle } from "@angular/cdk/drag-drop";
import { DOCUMENT, NgTemplateOutlet } from "@angular/common";
import {
  booleanAttribute,
  ChangeDetectionStrategy,
  Component,
  computed,
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
 * The modal dialog. Three things here look simplifiable but are not:
 *
 * 1. `descendants: false` on the content queries. `contentChild()` defaults to true, so a nested
 *    `ui-accordion` / `ui-data-table`'s own `#header` / `#content` templates would satisfy the
 *    DIALOG's queries — it would render the accordion's header as its title.
 *
 * 2. `disableClose: true` plus our own Escape handler, rather than `[disableClose]="!closable()"`.
 *    Brain gates Escape and backdrop-click on that one flag, and its backdrop click is otherwise
 *    ungated, so binding it would give every dialog click-outside-to-discard; there is no "escape
 *    closes, backdrop does not" option. It also lets us read `closable` live, which brain's open-time
 *    options snapshot cannot.
 *
 * 3. The width is an inline style on `hlm-dialog-content`, not on the host: CDK portals the panel out
 *    of this component's view, so a width on `<ui-dialog>` is a no-op and the panel stays clamped to
 *    Helm's `sm:max-w-md`.
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
    // On `document`: the panel is portalled into the CDK overlay container, outside this component's
    // DOM, so a host-scoped keydown would never see the event.
    "(document:keydown.escape)": "onEscape()",
  },
})
export class UiDialog {
  private readonly document = inject(DOCUMENT);

  /** Distinguishes this instance's panel for the `breakpoints` stylesheet and the a11y label. */
  private readonly uid = `ui-dialog-${nextDialogId++}`;
  protected readonly panelId = `${this.uid}-panel`;
  protected readonly titleId = `${this.uid}-title`;

  public readonly open = model(false);

  public readonly header = input<string>("");

  /** e.g. `450px`, `32rem`, `80vw`. Unset -> Helm's default panel width. */
  public readonly width = input<string>();

  public readonly maxWidth = input<string>();

  /** Max-width media queries, e.g. `{ '640px': '95vw' }`. */
  public readonly breakpoints = input<Record<string, string>>();

  /** Shows the X, and gates Escape. */
  public readonly closable = input(true, { transform: booleanAttribute });

  public readonly draggable = input(true, { transform: booleanAttribute });

  public readonly resizable = input(true, { transform: booleanAttribute });

  /** Drives the backdrop. */
  public readonly modal = input(true, { transform: booleanAttribute });

  /**
   * Two events, not brain's single `stateChanged`: that fires on both transitions, so folding them
   * together would run every close handler on open as well — and most of them reset the form.
   */
  public readonly opened = output<void>();
  public readonly closed = output<void>();

  /* `descendants: false` is load-bearing — see the class comment. */
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

  protected readonly panelWidth = computed(() => {
    const resized = this.resizedWidth();
    return resized === null ? this.width() : `${resized}px`;
  });

  protected readonly panelHeight = computed(() => {
    const resized = this.resizedHeight();
    return resized === null ? undefined : `${resized}px`;
  });

  /** An inline width must also replace Helm's `sm:max-w-md`, or it clamps the panel. */
  protected readonly panelMaxWidth = computed(
    () => this.maxWidth() ?? (this.width() ? "calc(100vw - 2rem)" : undefined),
  );

  protected readonly state = computed(
    () => (this.open() ? "open" : "closed") satisfies BrnDialogState,
  );

  constructor() {
    // A media query cannot be an inline style, so `breakpoints` compiles to a stylesheet scoped by the
    // panel id. It must out-rank the inline width — hence `!important`.
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

  /** Brain fires this on both transitions; split it back into the two events call sites expect. */
  protected onStateChanged(state: BrnDialogState): void {
    if (state === "open") {
      this.opened.emit();
      return;
    }

    // A close can originate inside the overlay (the X, Escape), not only from the caller flipping
    // `open`, so push the state back out before announcing it — that is what makes `[(open)]` work.
    this.resizedWidth.set(null);
    this.resizedHeight.set(null);
    this.open.set(false);
    this.closed.emit();
  }

  /**
   * `isTopmostOverlay` is not optional: a dropdown opened inside this dialog is its own stacked CDK
   * overlay, and its Escape must not reach us — otherwise one Escape dismisses the dropdown and
   * discards the form underneath it.
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

    // The overlay centres the panel, so it grows from both edges and the right edge only advances by
    // half of whatever we add to the width. Doubling the delta keeps the grip under the pointer.
    const onMove = (move: PointerEvent) => {
      this.resizedWidth.set(Math.max(MIN_RESIZE_WIDTH, width + (move.clientX - startX) * 2));
      this.resizedHeight.set(Math.max(MIN_RESIZE_HEIGHT, height + (move.clientY - startY) * 2));
    };
    const stop = () => {
      this.document.removeEventListener("pointermove", onMove);
      this.document.removeEventListener("pointerup", stop);
      this.document.removeEventListener("pointercancel", stop);
    };

    // Listen on the document, not on the grip, and do not use pointer capture: `setPointerCapture()`
    // throws NotFoundError whenever the pointerId is not a live pointer, and one throw before the
    // listeners are wired leaves the grip doing nothing at all. Document-level listeners already keep
    // receiving moves after the pointer leaves the 16px grip, and `pointercancel` covers the pointer
    // being torn away.
    this.document.addEventListener("pointermove", onMove);
    this.document.addEventListener("pointerup", stop);
    this.document.addEventListener("pointercancel", stop);
  }
}
