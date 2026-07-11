import { Directive, input } from "@angular/core";
import { BrnTabsTrigger } from "@spartan-ng/brain/tabs";
import { classes } from "../../utils";

/**
 * VENDOR MODIFICATION: the generator inlines this list in the constructor. It is lifted to an exported
 * const so `ui-tab` can wear the styling WITHOUT composing the directive.
 *
 * Why `ui-tab` cannot simply `hostDirectives: [HlmTabsTrigger]`: a Helm directive used as a host
 * directive on an exported component becomes part of that component's public API, and ng-packagr then
 * demands the private primitive be re-exported from `@logistics/shared/ui` (NG3001 — the same wall
 * `ui-skeleton` and `ui-progress` hit). A `const` carries no such constraint, so `ui-tab` host-directives
 * brain's `BrnTabsTrigger` (external, already public) and applies these classes.
 *
 * And the trigger MUST live on `ui-tab`'s host: `BrnTabsList` collects its triggers with a CONTENT
 * query, which cannot descend into a child component's view. Tucking a `<button hlmTabsTrigger>` inside
 * `ui-tab`'s template yields an empty trigger list and silently kills arrow-key navigation between tabs.
 */
export const TABS_TRIGGER_CLASSES = [
  `gap-1.5 rounded-md border border-transparent px-2 py-1 text-sm font-medium group-data-[variant=default]/tabs-list:data-active:shadow-sm group-data-[variant=line]/tabs-list:data-active:shadow-none [&_ng-icon:not([class*='text-'])]:text-[length:--spacing(4)] focus-visible:border-ring focus-visible:ring-ring/50 focus-visible:outline-ring text-foreground/60 hover:text-foreground dark:text-muted-foreground dark:hover:text-foreground relative inline-flex h-[calc(100%-1px)] flex-1 items-center justify-center whitespace-nowrap transition-all group-data-[orientation=vertical]/tabs:w-full group-data-[orientation=vertical]/tabs:justify-start focus-visible:ring-[3px] focus-visible:outline-1 disabled:pointer-events-none disabled:opacity-50 [&_ng-icon]:pointer-events-none [&_ng-icon]:shrink-0`,
  "group-data-[variant=line]/tabs-list:bg-transparent group-data-[variant=line]/tabs-list:data-active:bg-transparent dark:group-data-[variant=line]/tabs-list:data-active:border-transparent dark:group-data-[variant=line]/tabs-list:data-active:bg-transparent",
  "data-active:bg-background dark:data-active:text-foreground dark:data-active:border-input dark:data-active:bg-input/30 data-active:text-foreground",
  "after:bg-foreground after:absolute after:opacity-0 after:transition-opacity group-data-[orientation=horizontal]/tabs:after:inset-x-0 group-data-[orientation=horizontal]/tabs:after:bottom-[-5px] group-data-[orientation=horizontal]/tabs:after:h-0.5 group-data-[orientation=vertical]/tabs:after:inset-y-0 group-data-[orientation=vertical]/tabs:after:-right-1 group-data-[orientation=vertical]/tabs:after:w-0.5 group-data-[variant=line]/tabs-list:data-active:after:opacity-100",
];

@Directive({
  selector: "[hlmTabsTrigger]",
  hostDirectives: [
    { directive: BrnTabsTrigger, inputs: ["brnTabsTrigger: hlmTabsTrigger", "disabled"] },
  ],
  host: {
    "data-slot": "tabs-trigger",
  },
})
export class HlmTabsTrigger {
  public readonly triggerFor = input.required<string>({ alias: "hlmTabsTrigger" });
  constructor() {
    classes(() => TABS_TRIGGER_CLASSES);
  }
}
