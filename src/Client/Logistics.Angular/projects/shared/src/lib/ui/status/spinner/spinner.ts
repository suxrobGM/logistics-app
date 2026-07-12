import { Component, input } from "@angular/core";
import { Icon } from "../../icons/icon/icon";

/**
 * The busy spinner.
 *
 * =================================================================================================
 * THE DEFAULT IS 100px, AND THAT IS NOT A TYPO.
 * =================================================================================================
 * 50 of 63 call sites pass NOTHING, so whatever this component defaults to IS the spinner in fifty
 * places. Keeping 100px keeps the BOX, which is the part that would reflow fifty loading states if
 * it changed. Shrinking it to something more tasteful is a design decision, and it belongs in a
 * commit that is about the design — `size` is here so that commit is one-line per site.
 *
 * THE GLYPH renders the same spinning circle every other busy state in the app already uses
 * (`ui-button`'s loading glyph is this exact icon), in `currentColor`, so it inherits the
 * surrounding text colour and themes for free.
 *
 * WHY NOT `hlm-spinner` (we generated it, then deleted it)
 * It is a `<ng-icon>` on `lucideLoader2` with `animate-spin`, registered through its own local
 * `provideIcons`. We already render that glyph — `<ui-icon name="loader-circle" spin>` — and
 * `ui-button` already spins it. One icon pipeline.
 *
 * @example
 * <ui-spinner />                                   <!-- 100px default -->
 * <ui-spinner size="24px" ariaLabel="Loading loads" />
 */
@Component({
  selector: "ui-spinner",
  templateUrl: "./spinner.html",
  imports: [Icon],
  host: {
    // `role="status"` + a name is what makes a spinner announce itself. Every site gets a named one.
    role: "status",
    "[attr.aria-label]": "ariaLabel()",
    // inline-flex, not inline: `transform` does not apply to a non-replaced inline box, so
    // `animate-spin` on an inline host is a silent no-op. Same reason ui-icon's host is inline-flex.
    class: "inline-flex items-center justify-center",
    // The glyph is `size="inherit"` — it emits no text-* class and takes the host's font-size. So
    // sizing the host's FONT sizes the spinner, and the host's box matches it exactly.
    "[style.font-size]": "size()",
    "[style.width]": "size()",
    "[style.height]": "size()",
  },
})
export class Spinner {
  /** Any CSS length. Defaults to 100px (see above). */
  public readonly size = input<string>("100px");

  public readonly ariaLabel = input<string>("Loading");

  /*
   * There is deliberately no `strokeWidth` input: a Lucide glyph's stroke is baked into its path,
   * so there is nothing to parameterize, and it affects nothing about layout.
   */
}
