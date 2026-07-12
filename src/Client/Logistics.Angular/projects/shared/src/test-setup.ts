/**
 * JSDOM gaps the specs need patched.
 *
 * Supply MISSING APIs only. A shim that changes behaviour, rather than merely providing something a
 * real browser already has, turns the suite into a test of the shim.
 */

/**
 * JSDOM has no `matchMedia`. `BrnSonnerToaster` calls it from an `afterNextRender` hook, so unpatched
 * it throws DURING RENDER, tearing the toaster down and failing every toast assertion for a reason
 * unrelated to toasts.
 *
 * Reporting "no match" is faithful: the app themes from `--popover` / `--border`, which follow the
 * in-app `.dark-theme` class, so nothing under test depends on the OS preference.
 */
if (typeof window !== "undefined" && typeof window.matchMedia !== "function") {
  Object.defineProperty(window, "matchMedia", {
    writable: true,
    configurable: true,
    value: (query: string): MediaQueryList =>
      ({
        matches: false,
        media: query,
        onchange: null,
        addEventListener: () => undefined,
        removeEventListener: () => undefined,
        dispatchEvent: () => false,
        // Deprecated pre-EventTarget API; some libraries still feature-detect it.
        addListener: () => undefined,
        removeListener: () => undefined,
      }) as unknown as MediaQueryList,
  });
}

/**
 * JSDOM implements no scrolling, so `scrollIntoView` is absent. `BrnSelectItem` calls it whenever the
 * CDK key manager makes an option active — i.e. as soon as any `hlm-select` panel opens, including the
 * data table paginator's rows-per-page dropdown.
 *
 * A no-op is faithful: there is no viewport to scroll, and nothing under test asserts scroll position.
 */
if (typeof Element !== "undefined" && typeof Element.prototype.scrollIntoView !== "function") {
  Element.prototype.scrollIntoView = function scrollIntoView(): void {
    // No viewport in JSDOM; nothing to scroll.
  };
}
