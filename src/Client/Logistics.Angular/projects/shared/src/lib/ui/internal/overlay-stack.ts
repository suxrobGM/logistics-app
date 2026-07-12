/**
 * "Am I the overlay that owns this Escape?"
 *
 * INTERNAL. Not exported from `@logistics/shared/ui` — `ui-dialog` and `ui-confirm-dialog` are the
 * only consumers, and they are the only two components that arbitrate Escape themselves.
 *
 * =================================================================================================
 * WHY THIS EXISTS (a real data-loss bug, green on build / tests / lint, found only in a browser)
 * =================================================================================================
 * Both dialogs listen for Escape on the DOCUMENT, and they must: the panel is portalled into the CDK
 * overlay container, so a host-scoped listener would never see the key.
 *
 * But a `ui-select-field` / autocomplete / date-picker / menu / popover opened FROM INSIDE a dialog is
 * its OWN CDK overlay, stacked above the dialog's. Escape there means "close the dropdown", not "throw
 * away my half-filled form". Under PrimeNG that distinction came for free, because PrimeNG's own
 * select stops the event:
 *
 *     onEscapeKey(event) { ... event.preventDefault(); event.stopPropagation(); }
 *
 * so `p-dialog`'s document listener NEVER SAW the escape that dismissed a dropdown. Helm/brain's
 * overlays do not stop propagation — they close themselves through CDK's keyboard dispatcher and let
 * the event bubble on. So after the migration a single Escape closed the dropdown AND the dialog
 * underneath it: open the "Create a new load" dialog in the trip wizard, open any dropdown, press
 * Escape, and the whole form is gone. Verified in a browser; nothing else could have caught it.
 *
 * The same shape bites `ui-confirm-dialog` stacked on a `ui-dialog` — e.g. the customer edit dialog's
 * Danger Zone → "Delete Customer" → confirm. Escape on the confirm would also close the edit dialog.
 *
 * =================================================================================================
 * WHY DOM ORDER IS A SOUND TEST (measured, not assumed)
 * =================================================================================================
 * CDK appends each overlay's pane to `.cdk-overlay-container` in creation order, so a nested overlay's
 * pane always follows its parent's. The risk in reading the DOM would be a DETACHED overlay leaving a
 * stale pane behind us and permanently suppressing Escape — a worse bug than the one being fixed. It
 * does not: opening a select inside a dialog takes the container from 1 pane to 2, and closing the
 * select takes it back to 1, with nothing left over (measured in Chromium against the real trip-wizard
 * dialog). The `childElementCount` guard below is belt-and-braces on top of that, so an empty pane
 * could never suppress Escape even if a future CDK kept one around.
 */
export function isTopmostOverlay(element: Element | null | undefined): boolean {
  const pane = element?.closest(".cdk-overlay-pane");

  // Not portalled into an overlay (yet) — nothing can be stacked above us.
  if (!pane) return true;

  const panes = Array.from(pane.ownerDocument.querySelectorAll(".cdk-overlay-pane"));
  const mine = panes.indexOf(pane);

  // An empty pane is a detached overlay: it does not own the Escape.
  return !panes.slice(mine + 1).some((other) => other.childElementCount > 0);
}
