/**
 * Focus support for `FormValueControl` wrappers.
 *
 * Signal Forms' `FieldState.focusBoundControl()` (used by `ValidatedForm` to jump to the first
 * invalid field on submit) delegates to the bound control's optional `focus()` method. When a
 * control does NOT implement `focus()`, Angular falls back to calling `.focus()` on the
 * `[formField]` host element — which for a custom element like `<ui-text-field>` is a silent no-op,
 * because the host carries no `tabindex`.
 *
 * So every `ui-*-field` wrapper implements `focus()` and delegates here.
 *
 * @see FormUiControl.focus in @angular/forms/signals
 * @see signal-forms-v22-api-probe.spec.ts, claim L
 */

/**
 * Focusable descendants, in the order a wrapper is likely to render them. `[tabindex]` catches
 * widgets like `ui-select-field` / `ui-multiselect-field`, whose focus target is a `div`
 * (Helm's `hlm-select-trigger`), not an `input`.
 */
const FOCUSABLE = 'input:not([type="hidden"]), textarea, select, [tabindex]:not([tabindex="-1"])';

/** Focuses the first focusable element the wrapper renders. */
export function focusFirstControl(host: HTMLElement, options?: FocusOptions): void {
  const target = host.querySelector<HTMLElement>(FOCUSABLE);
  target?.focus(options);
}
