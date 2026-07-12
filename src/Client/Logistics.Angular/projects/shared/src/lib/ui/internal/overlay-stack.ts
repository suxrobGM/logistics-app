/**
 * "Am I the overlay that owns this Escape?" — internal, used by `ui-dialog` / `ui-confirm-dialog`.
 *
 * Both listen for Escape on the document, because their panel is portalled into the CDK overlay
 * container where a host-scoped listener would never see the key. But a select / date-picker / menu
 * opened from inside a dialog is its own stacked CDK overlay, and Escape there means "close the
 * dropdown", not "discard my half-filled form". Brain's overlays do not stop propagation, so without
 * this guard one Escape closes the dropdown and the dialog under it.
 *
 * DOM order is a sound test: CDK appends each pane to `.cdk-overlay-container` in creation order, so a
 * nested overlay's pane always follows its parent's.
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
