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
