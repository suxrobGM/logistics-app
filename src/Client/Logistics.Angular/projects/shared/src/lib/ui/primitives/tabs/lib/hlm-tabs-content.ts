import { Directive, input } from "@angular/core";
import { BrnTabsContent } from "@spartan-ng/brain/tabs";
import { classes } from "../../utils";

/** Lifted to a const so the `ui-*` wrapper can wear these classes without composing the DIRECTIVE (NG3001). */
export const TABS_CONTENT_CLASSES = "flex-1 text-sm outline-none";

@Directive({
  selector: "[hlmTabsContent]",
  hostDirectives: [{ directive: BrnTabsContent, inputs: ["brnTabsContent: hlmTabsContent"] }],
  host: {
    "data-slot": "tabs-content",
  },
})
export class HlmTabsContent {
  public readonly contentFor = input.required<string>({ alias: "hlmTabsContent" });

  constructor() {
    classes(() => TABS_CONTENT_CLASSES);
  }
}
