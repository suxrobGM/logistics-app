import { effect, signal, type ElementRef, type Signal } from "@angular/core";

/** Scroll distance from the bottom under which auto-scroll stays engaged. */
const ScrollPinThresholdPx = 48;

interface PinnedScroll {
  /** False while the user has scrolled up to read back - new content must not yank them down. */
  readonly pinnedToBottom: Signal<boolean>;
  onScroll(): void;
  scrollToBottom(): void;
}

/**
 * Keeps a chat transcript stuck to its newest entry, the way both agent surfaces want it. Call from
 * an injection context: it registers an effect that re-follows whenever `track` reports new content.
 */
export function pinnedScroll(
  container: Signal<ElementRef<HTMLElement> | undefined>,
  track: () => void,
): PinnedScroll {
  const pinnedToBottom = signal(true);

  effect(() => {
    track();
    if (!pinnedToBottom()) return;
    const el = container()?.nativeElement;
    // The new content only lands in the DOM after this effect, so measuring now would come up short.
    if (el) queueMicrotask(() => el.scrollTo({ top: el.scrollHeight }));
  });

  return {
    pinnedToBottom: pinnedToBottom.asReadonly(),

    onScroll(): void {
      const el = container()?.nativeElement;
      if (!el) return;
      pinnedToBottom.set(el.scrollHeight - el.scrollTop - el.clientHeight < ScrollPinThresholdPx);
    },

    scrollToBottom(): void {
      const el = container()?.nativeElement;
      if (!el) return;
      el.scrollTo({ top: el.scrollHeight });
      pinnedToBottom.set(true);
    },
  };
}
