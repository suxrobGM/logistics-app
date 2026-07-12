/**
 * Finding the element that actually takes focus inside a wrapper. Internal.
 *
 * `ui-button` renders a real `<button>` inside its host, so a directive applied to `<ui-button>` sits
 * on the wrapper, which has no tabindex. Anything needing the focusable element (a tooltip's
 * `aria-describedby`, a menu refocusing its trigger) must resolve down to it or it silently targets an
 * element that can never be focused.
 *
 * Only the selector is shared: the callers keep their own resolution rules, because `uiTooltip`
 * deliberately leaves the description on a native non-focusable host such as `<span>` / `<td>`.
 */
export const FOCUSABLE_SELECTOR =
  'button, a[href], input, select, textarea, [tabindex]:not([tabindex="-1"])';

export function isFocusable(element: HTMLElement): boolean {
  return element.tabIndex >= 0 || element.matches(FOCUSABLE_SELECTOR);
}

export function firstFocusableIn(host: HTMLElement): HTMLElement | null {
  return host.querySelector<HTMLElement>(FOCUSABLE_SELECTOR);
}
