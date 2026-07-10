import { NgModule } from "@angular/core";
import {
  RowToggler,
  SelectableRow,
  TableCheckbox,
  TableHeaderCheckbox,
  TableModule,
} from "primeng/table";

/**
 * Row-level behaviors for `<ui-data-table>`: `pSelectableRow`, `pRowToggler`, and the
 * selection checkboxes.
 *
 * These are re-exported rather than re-implemented. `SelectableRow` carries non-trivial keyboard
 * navigation (arrow/home/end/space/enter) and anchor-row tracking, and the checkboxes coordinate
 * "select all" against the table's `dataKey` — all easy to get subtly wrong. None of them are
 * standalone, so they cannot be listed in a component's `imports` directly or applied via
 * `hostDirectives`.
 *
 * Importing this module keeps `primeng/table` out of feature code — the swap in Phase 7 changes
 * this file, not the templates that use it.
 *
 * They rely on injecting PrimeNG's `Table`, which `UiDataTable` provides from its own view.
 *
 * @example
 * <ng-template #body let-row>
 *   <tr [pSelectableRow]="row">
 *     <td><p-tableCheckbox [value]="row" /></td>
 *     <td><button type="button" [pRowToggler]="row">…</button></td>
 *   </tr>
 * </ng-template>
 */
@NgModule({
  imports: [TableModule],
  exports: [SelectableRow, RowToggler, TableCheckbox, TableHeaderCheckbox],
})
export class UiTableRowDirectives {}
