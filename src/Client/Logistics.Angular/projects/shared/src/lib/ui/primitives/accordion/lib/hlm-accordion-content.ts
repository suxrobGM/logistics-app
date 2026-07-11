import { ChangeDetectionStrategy, Component } from "@angular/core";
import { BrnAccordionContent } from "@spartan-ng/brain/accordion";
import { classes } from "../../utils";

@Component({
  selector: "hlm-accordion-content",
  changeDetection: ChangeDetectionStrategy.OnPush,
  hostDirectives: [{ directive: BrnAccordionContent, inputs: ["style"] }],
  host: {
    "data-slot": "accordion-content",
  },
  template: `
    <div
      class="[&_a]:hover:text-foreground pt-0 pb-4 [&_a]:underline [&_a]:underline-offset-3 [&_p:not(:last-child)]:mb-4"
    >
      <ng-content />
    </div>
  `,
})
export class HlmAccordionContent {
  constructor() {
    classes(
      () =>
        "text-sm transition-all data-[state=closed]:h-0 data-[state=open]:h-(--brn-accordion-content-height)",
    );
  }
}
