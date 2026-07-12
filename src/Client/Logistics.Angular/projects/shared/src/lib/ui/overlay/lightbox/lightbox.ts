import { booleanAttribute, Component, computed, input, model } from "@angular/core";
import { UiButton } from "../../action/button";
import { Icon } from "../../icons/icon/icon";
import { UiDialog } from "../dialog/dialog";

/** One photo in a {@link UiLightbox}. */
export interface UiLightboxImage {
  src: string;
  /** Falls back to `src` when absent. */
  thumbnailSrc?: string;
  alt?: string;
}

/**
 * Fullscreen photo viewer. Replaces `<p-galleria>` — one call site, the condition-report photo strip.
 *
 * Built on `ui-dialog` rather than hand-rolled, so it inherits the overlay, the backdrop, the focus
 * trap, and — the part that matters — the Escape arbitration in `internal/overlay-stack.ts`. A
 * lightbox that swallowed Escape from a dropdown stacked above it would be the same bug that guard
 * exists to prevent.
 *
 * Keyboard: Left/Right (and Home/End) move between photos; Escape closes, via `ui-dialog`.
 *
 * `circular` wraps past the ends, matching the `[circular]="true"` the call site passed. With one
 * photo the navigators hide themselves — there is nowhere to go.
 */
@Component({
  selector: "ui-lightbox",
  templateUrl: "./lightbox.html",
  imports: [UiDialog, UiButton, Icon],
  host: {
    // Arrow keys are read from the document because the dialog's panel is portalled into the CDK
    // overlay container — a host-scoped listener would never see the key. Guarded on `open()` so a
    // closed lightbox does not eat arrow keys from the page behind it.
    "(document:keydown)": "onKeydown($event)",
  },
})
export class UiLightbox {
  public readonly images = input<readonly UiLightboxImage[]>([]);
  public readonly open = model(false);
  public readonly activeIndex = model(0);

  public readonly circular = input(true, { transform: booleanAttribute });
  public readonly showThumbnails = input(true, { transform: booleanAttribute });
  public readonly showNavigators = input(true, { transform: booleanAttribute });

  protected readonly count = computed(() => this.images().length);

  /** Clamped, so an out-of-range `activeIndex` from a caller cannot blank the view. */
  protected readonly current = computed(() => {
    const images = this.images();
    if (images.length === 0) {
      return null;
    }
    const index = Math.min(Math.max(this.activeIndex(), 0), images.length - 1);
    return images[index];
  });

  /** Nowhere to navigate with a single photo. */
  protected readonly navigable = computed(() => this.showNavigators() && this.count() > 1);

  protected go(delta: number): void {
    const count = this.count();
    if (count === 0) {
      return;
    }
    const next = this.activeIndex() + delta;
    if (this.circular()) {
      this.activeIndex.set((next + count) % count);
      return;
    }
    this.activeIndex.set(Math.min(Math.max(next, 0), count - 1));
  }

  protected select(index: number): void {
    this.activeIndex.set(index);
  }

  protected onKeydown(event: KeyboardEvent): void {
    if (!this.open() || this.count() === 0) {
      return;
    }

    switch (event.key) {
      case "ArrowLeft":
        this.go(-1);
        break;
      case "ArrowRight":
        this.go(1);
        break;
      case "Home":
        this.activeIndex.set(0);
        break;
      case "End":
        this.activeIndex.set(this.count() - 1);
        break;
      default:
        return;
    }
    // Only reached when we handled the key: stop the page behind from also scrolling.
    event.preventDefault();
  }
}
