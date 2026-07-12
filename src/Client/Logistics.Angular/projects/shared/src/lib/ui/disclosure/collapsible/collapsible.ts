import { booleanAttribute, ChangeDetectionStrategy, Component, input, model } from "@angular/core";
import { Icon } from "../../icons/icon/icon";
import {
  HlmCollapsible,
  HlmCollapsibleContent,
  HlmCollapsibleTrigger,
} from "../../primitives/collapsible";

/**
 * The collapsible fieldset. Replaces `<p-fieldset>` at 5 call sites / 2 files (load-form ×4,
 * truck-hazmat-section ×1). Every one of them is `legend` + `[toggleable]` + `[collapsed]`, so this is
 * BEHAVIOURAL, not cosmetic.
 *
 * THE CONTENT IS HIDDEN, NEVER DESTROYED — and that is the whole ballgame.
 * All four load-form sections and the hazmat section wrap SIGNAL-FORMS FIELDS. p-fieldset keeps its
 * content container in the DOM when collapsed (it animates height; there is no `*ngIf` on it), so
 * those fields stay registered with the form and keep contributing their validity. Swap in an `@if`
 * and a collapsed "Schedule" section silently drops `requestedPickupDate` out of the form — the load
 * saves without it, and nothing anywhere goes red.
 *
 * `HlmCollapsibleContent` behaves the same way: `data-[state=closed]:hidden` is `display: none`, and
 * brain additionally marks the region `inert` while closed, so the hidden fields are not tab-reachable
 * either. Same DOM lifetime as p-fieldset, better a11y.
 *
 * `[collapsed]` IS A SEED, NOT A CONTROLLED PROP. p-fieldset takes `collapsed` as a plain `@Input`
 * with a setter over private `_collapsed` state, so Angular only writes it when the bound EXPRESSION
 * changes value. A user's manual toggle therefore sticks until the expression itself changes. The four
 * one-way call sites — `[collapsed]="true"`, `[collapsed]="!model().isHazmat"` — depend on that: made
 * a controlled prop instead, the section would snap shut again on the very next change detection and
 * the user could never open it. A `model()` reproduces the p-fieldset behaviour exactly.
 */
@Component({
  selector: "ui-collapsible",
  templateUrl: "./collapsible.html",
  imports: [HlmCollapsible, HlmCollapsibleTrigger, HlmCollapsibleContent, Icon],
  changeDetection: ChangeDetectionStrategy.OnPush,
})
export class UiCollapsible {
  /** p-fieldset's `legend`. Static at all 5 call sites. */
  public readonly legend = input<string>("");

  /** p-fieldset's `toggleable` (default false). All 5 call sites pass `true`. */
  public readonly toggleable = input(false, { transform: booleanAttribute });

  /** p-fieldset's `collapsed`. See the class comment: a seed, not a controlled prop. */
  public readonly collapsed = model(false);

  /** brain speaks `expanded`; p-fieldset speaks `collapsed`. Invert at the boundary, once. */
  protected readonly expanded = () => !this.collapsed();

  protected onExpandedChange(expanded: boolean): void {
    this.collapsed.set(!expanded);
  }
}
