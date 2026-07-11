import { DatePipe } from "@angular/common";
import { Component, computed, signal } from "@angular/core";
import {
  StatusBadge,
  Typography,
  UiDataTable,
  UiSortHeader,
  UiTableRowDirectives,
} from "@logistics/shared/ui";
import { LAB_SELECTABLE_SHIPMENTS, LAB_SHIPMENTS, type LabShipment } from "../fixtures/shipments";

/**
 * `<ui-data-table>` in its two real modes, against a 57-row local fixture.
 *
 * Table 1 is client-side (paginator, no `lazy`) so the table's *own* comparator runs — which is
 * what makes the `code` column meaningful. Sorting it ascending must give 1, 2, … 9, 10 … 100,
 * not "1", "10", "100", "1000", "101", … "9". Nothing in the type system or the unit tests notices
 * the difference; a human (or a screenshot diff) looking at this column does.
 *
 * Table 2 is the selection mode: `dataKey` + `selectionMode="multiple"` + the header/row
 * checkboxes. The live `selection()` is printed next to it as JSON so "select all" semantics
 * (does it select the page, or the whole 57 rows?) are readable off the screen.
 */
@Component({
  selector: "app-ui-lab-table",
  templateUrl: "./table-section.html",
  imports: [DatePipe, StatusBadge, Typography, UiDataTable, UiSortHeader, UiTableRowDirectives],
})
export class UiLabTableSection {
  protected readonly shipments = LAB_SHIPMENTS;
  protected readonly selectableShipments = LAB_SELECTABLE_SHIPMENTS;

  protected readonly selection = signal<LabShipment[]>([]);

  /**
   * Ids only — the rows carry `Date`s, and `JSON.stringify` on the whole row would bury the one
   * thing we are checking (which rows the table thinks are selected) in noise.
   */
  protected readonly selectionJson = computed(() =>
    JSON.stringify(
      {
        count: this.selection().length,
        ids: this.selection().map((row) => row.id),
      },
      null,
      2,
    ),
  );

  protected clearSelection(): void {
    this.selection.set([]);
  }
}
