import { Component } from "@angular/core";
import { HlmAccordionContent } from "../../primitives/accordion";

/**
 * The collapsible body. Brain animates its height and sets `inert` when closed.
 *
 * `display: contents` IS LOAD-BEARING — a wrapper element here changes layout.
 *
 * Helm collapses the panel with `data-[state=closed]:h-0` + `overflow-hidden` on `<hlm-accordion-content>`.
 * That is an UNKNOWN element, so its default display is `inline` — and height and overflow do not
 * apply to an inline box. It only works upstream because it is a direct child of the accordion item,
 * whose class list is `flex flex-col`: flex items get BLOCKIFIED, which is what gives the height
 * something to bite on.
 *
 * Interposing this wrapper severed that. `<hlm-accordion-content>` became an inline child of a plain
 * `<ui-accordion-content>`, `h-0` did nothing, and EVERY PANEL RENDERED PERMANENTLY OPEN — no error,
 * correct `aria-expanded`, correct `data-state`, correct `inert`, and content on screen regardless.
 * `contents` removes this box from the layout so the Helm element is once again a flex item of the
 * panel. (Caught in a browser; nothing else would have.)
 */
@Component({
  selector: "ui-accordion-content",
  templateUrl: "./accordion-content.html",
  imports: [HlmAccordionContent],
  host: { class: "contents" },
})
export class UiAccordionContent {}
