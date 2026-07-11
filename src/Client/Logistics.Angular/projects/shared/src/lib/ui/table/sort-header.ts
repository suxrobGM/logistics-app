import { Component, computed, HostAttributeToken, inject } from "@angular/core";
import { Icon } from "../content/icon/icon";
import type { IconName } from "../icons/icon-registry.generated";
import { UiTableState } from "./table-state";

/**
 * A sortable column header for `<ui-data-table>`.
 *
 * Injects the enclosing table's {@link UiTableState}. That works from inside a CONSUMER's
 * `<ng-template #header>` because the template is declared as a child node of the
 * `<ui-data-table>` element and an embedded view's injector is parented at its DECLARATION node —
 * so a nested table's headers resolve the nested state, not the outer one. Pinned by
 * `di-shadowing.probe.spec.ts`.
 *
 * The field is a static attribute (`uiSortHeader="Name"`), which is how all ~50 call sites use it:
 * sort fields are column names, not expressions.
 *
 * ⚠ **It renders its own arrow.** PrimeNG's came from `<p-sortIcon>` and the `p-datatable-*`
 * classes that die with the preset — and a sort header with no arrow passes every test and every
 * code review while looking broken to every user.
 *
 * @example
 * <th uiSortHeader="Name">Name</th>
 */
@Component({
  selector: "th[uiSortHeader]",
  imports: [Icon],
  template: `
    <span class="inline-flex items-center gap-1">
      <ng-content />
      <ui-icon
        [name]="sortIcon()"
        size="xs"
        [class]="sorted() ? 'text-foreground' : 'text-muted-foreground/60'"
      />
    </span>
  `,
  host: {
    role: "columnheader",
    tabindex: "0",
    class: "cursor-pointer select-none",
    "[attr.aria-sort]": "ariaSort()",
    "(click)": "sort()",
    "(keydown.enter)": "sort()",
    "(keydown.space)": "onSpace($event)",
  },
})
export class UiSortHeader {
  private readonly state = inject(UiTableState);

  /** The field this column sorts by. */
  protected readonly field = inject(new HostAttributeToken("uiSortHeader"));

  protected readonly sorted = computed(() => this.state.isSortedBy(this.field));

  protected readonly ascending = computed(() => this.sorted() && (this.state.sortOrder() ?? 1) > 0);

  /**
   * ⚠ Typed `IconName`, and the unsorted arrow is returned as a BARE literal on its own `return`.
   *
   * Both details are load-bearing for the icon GATE, not just for types. `gen-icon-registry.mjs`
   * decides which glyphs each portal registers by REGEX-SCANNING the source; a `[name]="sortIcon()"`
   * binding is invisible to it, so the name has to be discoverable another way. All three of this
   * component's glyphs are therefore also pinned in that script's `MANDATORY_BASE` — the list for
   * icons a scan cannot see. Until they were, `chevrons-up-down` resolved through `ICON_ALIASES`
   * (so this compiled and check-icons passed) while `provideIcons` never registered it, and every
   * unsorted column header in all four portals rendered a blank 12x12 box.
   */
  protected readonly sortIcon = computed<IconName>(() => {
    if (!this.sorted()) {
      return "chevrons-up-down";
    }
    return this.ascending() ? "chevron-up" : "chevron-down";
  });

  protected readonly ariaSort = computed<"ascending" | "descending" | "none">(() => {
    if (!this.sorted()) {
      return "none";
    }
    return this.ascending() ? "ascending" : "descending";
  });

  protected sort(): void {
    this.state.sort(this.field);
  }

  /** Space must sort without also scrolling the page. */
  protected onSpace(event: Event): void {
    event.preventDefault();
    this.sort();
  }
}
