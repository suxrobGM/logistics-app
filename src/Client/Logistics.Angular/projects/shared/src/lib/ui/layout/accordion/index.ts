import { UiAccordion } from "./accordion";
import { UiAccordionContent } from "./accordion-content";
import { UiAccordionHeader } from "./accordion-header";
import { UiAccordionPanel } from "./accordion-panel";

export * from "./accordion";
export * from "./accordion-content";
export * from "./accordion-header";
export * from "./accordion-panel";

/** All four accordion elements. */
export const UiAccordionImports = [
  UiAccordion,
  UiAccordionPanel,
  UiAccordionHeader,
  UiAccordionContent,
] as const;
