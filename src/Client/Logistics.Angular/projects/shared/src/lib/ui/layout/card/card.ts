import { NgTemplateOutlet } from "@angular/common";
import { Component, contentChild, input, TemplateRef } from "@angular/core";
import { classes } from "../../primitives/utils";

/**
 * The card. Replaces `<p-card>` at 227 call sites / 119 files.
 *
 * =================================================================================================
 * IT IS NOT "PLAIN CONTENT PROJECTION". THERE ARE 83 TEMPLATE SLOTS.
 * =================================================================================================
 * PrimeNG 21 retired `pTemplate="header"` and `<p-header>` in favour of plain template refs, so a
 * grep for the old spellings finds nothing and the card looks like a content-projection-only box. It
 * is not. Across the repo:
 *
 *     <ng-template #header>     71        <ng-template #subtitle>    4
 *     <ng-template #title>       8        <ng-template #footer>      3
 *
 * So this component takes them by `contentChild` and renders them through `ngTemplateOutlet`, exactly
 * as p-card does. That is a deliberate choice over Helm-style named `<ng-content select>` slots: the
 * slot form would require rewriting 83 call sites into wrapper elements — restructuring DOM inside
 * cards that pages lay out against — where template refs make the sweep a pure tag rename. The
 * old-fashioned API buys a mechanical, reviewable migration; it can be modernised later, alone.
 *
 * THE DOM IS p-card's, NODE FOR NODE (primeng-card.mjs):
 *
 *     <ui-card>                    ← the root. `.p-card` lived on p-card's host too, which is why
 *       <div card-header>          ← all 86 `class=` call sites lay out against THIS box.
 *       <div card-body>            ← header sits OUTSIDE the body, and so outside its padding
 *         <div card-title>         ← `header` (the string input, 43 sites) renders HERE, not in
 *         <div card-subtitle>        card-header. Getting that backwards moves 43 headings.
 *         <div card-content>       ← the default <ng-content>
 *         <div card-footer>
 *
 * WHY NOT `hlmCard` (we generated it, then deleted it)
 * Helm's card is shadcn's: `ring-1 rounded-xl shadow-xs py-6 gap-6 text-sm overflow-hidden`. Building
 * on it would mean overriding the ring, the radius, the shadow, the padding and the gap — everything
 * — and `text-sm` cannot be overridden at all, only replaced: p-card sets NO font-size, so a card
 * nested in a `text-xs` block inherits 12px today and would jump to 14px under Helm. There is nothing
 * left of Helm's card after a faithful port, so this stands alone. Same call, same reasons, as
 * `ui-button` vs `hlm-button`.
 *
 * PER-APP RADIUS AND SHADOW. TMS runs Nora (card radius 4px), the other three run Aura (12px). The
 * `--ui-*` variables carry that fork; see the block in shared/styles/theme.css.
 *
 * THE `--ui-card-*` VARIABLES ARE A PUBLIC HOOK. They are how the TMS dashboard makes a card fill its
 * gridster panel and scroll its content — see tms-portal/…/home/home.css. Custom properties inherit
 * straight through component boundaries, so a page can reach a card nested inside a child component
 * without `::ng-deep` and without the child exposing an input for it.
 */
@Component({
  selector: "ui-card",
  templateUrl: "./card.html",
  imports: [NgTemplateOutlet],
})
export class Card {
  /**
   * The card's heading (43 static + 2 bound call sites). Renders into `card-title`, NOT `card-header`
   * — that is where p-card put it, and the two boxes have different padding.
   *
   * A `#title` template wins over it, as in p-card (`*ngIf="header && !titleTemplate"`).
   */
  public readonly header = input<string | null>(null);

  /** p-card's `subheader`. Zero call sites use it today; kept so `#subtitle`'s sibling exists. */
  public readonly subheader = input<string | null>(null);

  /* ===============================================================================================
   * `descendants: false` IS LOAD-BEARING. Do not drop it "because the default is fine".
   * ===============================================================================================
   * p-card declares every one of these five queries EXPLICITLY non-descending:
   *
   *     @ContentChild('header', { descendants: false }) headerTemplate;      — primeng-card.mjs
   *     @ContentChild('title',  { descendants: false }) titleTemplate;
   *     ... and the same for subtitle / content / footer
   *
   * Angular's `contentChild()` defaults `descendants` to TRUE ("traverse all descendants in the
   * same template" — core.d.ts). Those two are not the same query, and the difference is not subtle:
   * `#header`, `#title`, `#footer` are also `ui-data-table`'s OWN template slots, and 48 pages nest a
   * table inside a card:
   *
   *     <ui-card>
   *       <ui-data-table>
   *         <ng-template #header> <tr><th>Name</th>…      ← the TABLE's header row
   *
   * The `<ng-template>` is a descendant of `<ui-card>` in the page's template, so a descending query
   * MATCHES IT. The card then renders the table's `<tr><th>` into `card-header` — orphaned <th>s
   * outside any <table> — and the column labels appear twice, once as a mangled row above the card.
   * The build is green, every test passes, and /customers reads "Name↑↓StatusEmail↑↓Phone↑↓".
   *
   * Only a template ref written as a DIRECT CHILD of <ui-card> is this card's slot. That is p-card's
   * contract, and it is the only one that can distinguish "the card's header" from "some nested
   * component's header".
   * =============================================================================================== */
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
    // On the HOST, twMerging the call site's `class` last — so `class="h-full border-default border"`
    // (and the 85 others) beat our base, deterministically rather than by stylesheet order.
    classes(
      () =>
        "bg-card text-card-foreground flex flex-col rounded-[var(--ui-radius-card)] " +
        "shadow-[var(--ui-card-shadow)] h-[var(--ui-card-h,auto)]",
    );
  }
}
