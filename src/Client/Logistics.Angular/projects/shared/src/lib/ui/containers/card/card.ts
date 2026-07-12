import { NgTemplateOutlet } from "@angular/common";
import { Component, contentChild, input, TemplateRef } from "@angular/core";
import { classes } from "../../primitives/utils";

/**
 * The card.
 *
 * Slots are taken as `<ng-template #header|#title|#subtitle|#footer>` refs rendered through
 * `ngTemplateOutlet`, not as named `<ng-content select>` slots. DOM order matters: `card-header`
 * sits OUTSIDE `card-body` (different padding), and the `header` string input renders into
 * `card-title` inside the body — not into `card-header`.
 *
 * The `--ui-card-*` variables are a public hook: they are how the TMS dashboard makes a card fill
 * its gridster panel and scroll its content (see tms-portal/…/home/home.css). Custom properties
 * inherit through component boundaries, so a page can reach a card nested inside a child component
 * without `::ng-deep`.
 */
@Component({
  selector: "ui-card",
  templateUrl: "./card.html",
  imports: [NgTemplateOutlet],
})
export class Card {
  /** The card's heading. Renders into `card-title`, not `card-header`. A `#title` template wins. */
  public readonly header = input<string | null>(null);

  /** Sub-heading. No call sites use it today; kept so `#subtitle`'s string sibling exists. */
  public readonly subheader = input<string | null>(null);

  /**
   * `descendants: false` IS LOAD-BEARING — do not drop it "because the default is fine".
   * `contentChild()` defaults `descendants` to true, and `#header` / `#title` / `#footer` are also
   * `ui-data-table`'s own slot names. Many pages nest a table inside a card, so a descending query
   * matches the TABLE's `<ng-template #header>` and renders its `<tr><th>` into `card-header`,
   * orphaned outside any `<table>`. Only a template ref that is a DIRECT child of `<ui-card>` is
   * this card's slot.
   */
  protected readonly headerTpl = contentChild<TemplateRef<unknown>>("header", {
    descendants: false,
  });
  protected readonly titleTpl = contentChild<TemplateRef<unknown>>("title", { descendants: false });
  protected readonly subtitleTpl = contentChild<TemplateRef<unknown>>("subtitle", {
    descendants: false,
  });
  protected readonly footerTpl = contentChild<TemplateRef<unknown>>("footer", {
    descendants: false,
  });

  constructor() {
    // Lands on the HOST, twMerging the call site's `class` last so a call site beats our base
    // deterministically rather than by stylesheet order.
    classes(
      () =>
        "bg-card text-card-foreground flex flex-col rounded-[var(--ui-radius-card)] " +
        "shadow-[var(--ui-card-shadow)] h-[var(--ui-card-h,auto)]",
    );
  }
}
