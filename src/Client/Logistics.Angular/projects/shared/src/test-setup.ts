/**
 * Global test-environment shims for the `shared` library.
 *
 * These patch gaps in JSDOM — the headless DOM the specs run against. They must never patch anything
 * a real browser would not already provide, because a shim that changes BEHAVIOUR (rather than merely
 * supplying a missing API) turns the suite into a test of the shim.
 */

/**
 * JSDOM does not implement `window.matchMedia` at all — the property is simply absent.
 *
 * `BrnSonnerToaster` (the toast surface behind `ToastService`) calls
 * `matchMedia('(prefers-color-scheme: dark)').addEventListener(...)` from an `afterNextRender` hook to
 * follow the OS colour-scheme preference. Unpatched, that throws `matchMedia is not a function` DURING
 * RENDER, which tears the toaster down and makes every toast assertion fail for a reason that has
 * nothing to do with the toast.
 *
 * The stub reports "no match" (i.e. light) and accepts-but-never-fires listeners. That is a faithful
 * stand-in: the app does not read the OS preference anyway — it themes from `--popover` / `--border`,
 * which flip with the in-app `.dark-theme` class — so nothing under test depends on the answer.
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
 * JSDOM implements no scrolling at all, so `Element.prototype.scrollIntoView` is absent.
 *
 * `BrnSelectItem.setActiveStyles()` calls it whenever the CDK key manager makes an option active —
 * which happens as soon as any `hlm-select` panel opens, including the rows-per-page dropdown on
 * `<ui-data-table>`'s paginator. Unpatched it throws `scrollIntoView is not a function` from inside
 * an effect during the open, so the option never gets clicked and a page-size test fails for a
 * reason that has nothing to do with paging.
 *
 * Supplying the missing API only. Scrolling a viewport that does not exist is a genuine no-op here,
 * and nothing under test asserts on scroll position.
 */
if (typeof Element !== "undefined" && typeof Element.prototype.scrollIntoView !== "function") {
  Element.prototype.scrollIntoView = function scrollIntoView(): void {
    // No viewport in JSDOM; nothing to scroll.
  };
}
