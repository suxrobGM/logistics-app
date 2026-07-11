import { ChangeDetectionStrategy, Component, computed, input } from "@angular/core";
import { NgIcon, provideIcons } from "@ng-icons/core";
import { lucideChevronDown } from "@ng-icons/lucide";
import { BrnAccordionImports } from "@spartan-ng/brain/accordion";
import type { ClassValue } from "clsx";
import { hlm } from "../../utils";

/**
 * VENDOR MODIFICATION: ONE chevron that ROTATES, not two that show/hide.
 *
 * The stock generator renders a chevron-down and a chevron-up and swaps them with Tailwind DISPLAY
 * utilities (`group-aria-expanded/accordion-trigger:hidden` / `:inline`). In this repo that does not
 * work, and it fails in the worst way — BOTH CHEVRONS RENDER, on every accordion header, forever.
 *
 * Why: `@ng-icons`' `NgIcon` ships its own component style, `:host { display: inline-block; … }`.
 * Angular emits component styles UNLAYERED, and unlayered CSS beats every `@layer` regardless of
 * specificity — and Tailwind's utilities all live in `@layer utilities`. So `hidden` on an
 * `<ng-icon>` is a no-op. (Verified in the browser: adding a bare `hidden` class to an `<ng-icon>`
 * leaves `getComputedStyle().display` at `block`.) Nothing throws, the class is right there in the
 * DOM, and the CSS rule is right there in the stylesheet — it just loses the cascade.
 *
 * `rotate-180` is a TRANSFORM, and `NgIcon` sets no transform, so there is no layer fight to lose.
 * This is also what shadcn's own accordion does, so we land on the canonical look rather than a
 * workaround. The rule generalises: never hide an `<ng-icon>` with a Tailwind display utility.
 */
@Component({
  selector: "hlm-accordion-trigger",
  imports: [BrnAccordionImports, NgIcon],
  providers: [provideIcons({ lucideChevronDown })],
  changeDetection: ChangeDetectionStrategy.OnPush,
  template: `
    <h3 brnAccordionHeader class="flex">
      <button brnAccordionTrigger data-slot="accordion-trigger" [class]="_computedTriggerClass()">
        <ng-content />
        <ng-icon
          name="lucideChevronDown"
          data-slot="accordion-trigger-icon"
          class="pointer-events-none shrink-0 transition-transform duration-200 group-aria-expanded/accordion-trigger:rotate-180"
        />
      </button>
    </h3>
  `,
})
export class HlmAccordionTrigger {
  public readonly triggerClass = input<ClassValue>("");

  protected readonly _computedTriggerClass = computed(() =>
    hlm(
      "focus-visible:ring-ring/50 focus-visible:border-ring focus-visible:after:border-ring **:data-[slot=accordion-trigger-icon]:text-muted-foreground! rounded-md py-4 text-start text-sm font-medium hover:underline focus-visible:ring-3 **:data-[slot=accordion-trigger-icon]:ms-auto **:data-[slot=accordion-trigger-icon]:text-[length:--spacing(4)] group/accordion-trigger relative flex flex-1 items-start justify-between border border-transparent transition-all outline-none aria-disabled:pointer-events-none aria-disabled:opacity-50",
      this.triggerClass(),
    ),
  );
}
