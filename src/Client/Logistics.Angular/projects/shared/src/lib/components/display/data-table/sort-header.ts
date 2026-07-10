import { Component, DestroyRef, HostAttributeToken, inject, signal } from "@angular/core";
import { takeUntilDestroyed } from "@angular/core/rxjs-interop";
import { Table, TableModule } from "primeng/table";

/**
 * A sortable column header for `<ui-data-table>`.
 *
 * Replaces the `pSortableColumn` + `<p-sortIcon>` pair that feature templates used to write by
 * hand, so nothing outside this directory imports `primeng/table`. Swapping the table
 * implementation is then a change to this file and `UiDataTable`, not to the 82 templates.
 *
 * PrimeNG's own `SortableColumn` cannot be reused via `hostDirectives` because it is declared in
 * `TableModule` rather than being standalone. This reproduces its exact contract instead: the
 * same `p-datatable-sortable-column` / `p-datatable-column-sorted` classes, the same
 * `role="columnheader"` / `tabindex` / `aria-sort` attributes, and the same click, Enter and
 * Space handling — all delegating to `Table.sort()`.
 *
 * The field must be a static attribute (`uiSortHeader="Name"`), which is how every call site uses
 * it: sort fields are server-side column names, not expressions.
 *
 * @example
 * <th uiSortHeader="Name">Name</th>
 */
@Component({
  selector: "th[uiSortHeader]",
  imports: [TableModule],
  template: `<ng-content /><p-sortIcon [field]="field" />`,
  host: {
    role: "columnheader",
    tabindex: "0",
    class: "p-datatable-sortable-column",
    "[class.p-datatable-column-sorted]": "sorted()",
    "[attr.aria-sort]": "ariaSort()",
    "(click)": "sort($event)",
    "(keydown.enter)": "sort($event)",
    "(keydown.space)": "sort($event)",
  },
})
export class UiSortHeader {
  private readonly table = inject(Table);

  /** The server-side field name this column sorts by. */
  protected readonly field = inject(new HostAttributeToken("uiSortHeader"));

  protected readonly sorted = signal(false);
  protected readonly ariaSort = signal<"ascending" | "descending" | "none">("none");

  constructor() {
    // The table announces every sort; recompute this column's state from it, exactly as
    // PrimeNG's SortableColumn does.
    this.table.tableService.sortSource$
      .pipe(takeUntilDestroyed(inject(DestroyRef)))
      .subscribe(() => this.updateSortState());
    this.updateSortState();
  }

  protected sort(event: Event): void {
    this.table.sort({ originalEvent: event, field: this.field });
    this.updateSortState();
  }

  private updateSortState(): void {
    const isSorted = !!this.table.isSorted(this.field);
    this.sorted.set(isSorted);
    this.ariaSort.set(!isSorted ? "none" : this.table.sortOrder === 1 ? "ascending" : "descending");
  }
}
