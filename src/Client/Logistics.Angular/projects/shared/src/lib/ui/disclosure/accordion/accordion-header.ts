import { Component } from "@angular/core";
import { HlmAccordionTrigger } from "../../primitives/accordion";

/**
 * The clickable panel header. Replaces `<p-accordion-header>`.
 *
 * Helm's trigger renders the `<h3 brnAccordionHeader><button brnAccordionTrigger>` pair and the
 * chevron; it must stay nested inside the panel element so that button can reach `BrnAccordionItem`
 * and `BrnAccordion` up the element-injector chain.
 */
@Component({
  selector: "ui-accordion-header",
  templateUrl: "./accordion-header.html",
  imports: [HlmAccordionTrigger],
  // `contents` for the same reason as ui-accordion-content: keep this wrapper out of the panel's
  // `flex flex-col` layout so the Helm trigger stays a direct flex item of it.
  host: { class: "contents" },
})
export class UiAccordionHeader {}
