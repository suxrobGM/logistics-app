import { Component, effect, inject, input, output } from "@angular/core";
import { BrnTabs } from "@spartan-ng/brain/tabs";
import { TABS_ROOT_CLASSES } from "../../primitives/tabs";
import { classes } from "../../primitives/utils";
import { tabKey, type UiTabValue } from "./tab-value";

/**
 * Tabs. Replaces `<p-tabs>` at 7 call sites (31 tabs / 31 panels).
 *
 * The five-element shape (`ui-tabs` / `ui-tab-list` / `ui-tab` / `ui-tab-panels` / `ui-tab-panel`)
 * mirrors `<p-tabs>`'s, so every call site is a tag rename with `[value]` left alone.
 *
 * THE NUMBER/STRING TRAP — why `value` is normalised rather than forwarded.
 * `BrnTabs` selects by strict identity against each trigger's key (`$activeTab() === triggerFor()`),
 * and those keys are strings. Five of the seven call sites hold `signal(0)` and bind a NUMBER. Passed
 * through raw, `0 === "0"` is false for every trigger, so no tab is selected and the panel area
 * renders blank — no error, green build. `tabKey()` normalises on the way in. (`ui-tab` and
 * `ui-tab-panel` instead take their key as a plain string attribute, because a `hostDirective` input
 * cannot be transformed on its way to brain's required `string` input.)
 *
 * The way OUT matters just as much: `expense-add` guards its handler with
 * `if (typeof index === "number") this.activeTab.set(index)`. Emit a string there and the tab never
 * changes, again silently. So `valueChange` re-emits in the caller's own type.
 *
 * CONTROLLED **AND** UNCONTROLLED — both call-site shapes must keep working.
 * Two call sites (`accident-detail`, `mcp-integration-guide`) pass a static `value` and bind NO
 * `(valueChange)`; their tabs must still switch on click. So `BrnTabs` owns the active tab and `value`
 * only *seeds* it: the sync effect below depends solely on `value()`, so a user click — which moves
 * `BrnTabs.activeTab` but not `value` — cannot retrigger it and snap the tab back. Reading `activeTab`
 * inside that effect would have created exactly that loop and frozen those two pages on tab one.
 *
 * `BrnTabs` is host-directived HERE, on `<ui-tabs>` itself, rather than placed on a `<div>` in the
 * template: `ui-tab` / `ui-tab-panel` arrive by content projection, and a projected node's element
 * injector is parented at its DECLARATION site. It can reach a directive on the `<ui-tabs>` host; it
 * could never reach one nested inside this component's view.
 */
@Component({
  selector: "ui-tabs",
  templateUrl: "./tabs.html",
  hostDirectives: [{ directive: BrnTabs, inputs: ["orientation", "activationMode"] }],
  host: { "data-slot": "tabs" },
})
export class UiTabs {
  private readonly brn = inject(BrnTabs);

  public readonly value = input<UiTabValue | undefined>(undefined);
  public readonly valueChange = output<UiTabValue>();

  constructor() {
    classes(() => TABS_ROOT_CLASSES);

    effect(() => this.brn.activeTab.set(tabKey(this.value())));

    this.brn.tabActivated.subscribe((activated) => {
      const next = String(activated ?? "");
      // `value()` is still the pre-click value here, so this only filters genuine no-ops.
      if (next === tabKey(this.value())) return;
      this.valueChange.emit(typeof this.value() === "number" ? Number(next) : next);
    });
  }
}
