import { Component, input } from "@angular/core";
import { BrnTabsList } from "@spartan-ng/brain/tabs";
import { listVariants } from "../../primitives/tabs";
import { classes } from "../../primitives/utils";

export type UiTabListVariant = "default" | "line";

/**
 * The row of tab buttons. Replaces `<p-tablist>`.
 *
 * Hosts brain's `BrnTabsList` (which owns the arrow-key `FocusKeyManager` and `role="tablist"`) and
 * wears Helm's `listVariants` classes. It composes the brain directive rather than `HlmTabsList`
 * because a Helm host directive would force the private primitive public (NG3001).
 */
@Component({
  selector: "ui-tab-list",
  templateUrl: "./tab-list.html",
  hostDirectives: [BrnTabsList],
  host: { "data-slot": "tabs-list", "[attr.data-variant]": "variant()" },
})
export class UiTabList {
  public readonly variant = input<UiTabListVariant>("default");

  constructor() {
    classes(() => listVariants({ variant: this.variant() }));
  }
}
