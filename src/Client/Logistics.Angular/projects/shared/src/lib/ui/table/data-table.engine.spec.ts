/**
 * Regression tests for the hand-rolled `<ui-data-table>` engine.
 *
 * Separate from `data-table.contract.spec.ts` on purpose: that file is the characterization contract
 * and must pass UNCHANGED, while this one pins behaviour the contract does not reach.
 */
import { Component, provideZonelessChangeDetection, signal } from "@angular/core";
import { TestBed, type ComponentFixture } from "@angular/core/testing";
import type { ListLazyLoadEvent } from "../../stores";
import { UiDataTable } from "./data-table";
import { UiSortHeader } from "./sort-header";
import { UiTableRowDirectives } from "./table-row-directives";

interface Row {
  id: number;
  name: string;
  customer: { name: string };
}

function rows(n: number): Row[] {
  return Array.from({ length: n }, (_, i) => ({
    id: i + 1,
    name: `Row ${i + 1}`,
    customer: { name: `Customer ${i + 1}` },
  }));
}

@Component({
  selector: "ui-host-engine-table",
  imports: [UiDataTable, UiSortHeader, UiTableRowDirectives],
  template: `
    <ui-data-table
      [value]="data()"
      [paginator]="true"
      [rows]="10"
      [rowsPerPageOptions]="[10, 25, 50]"
      [globalFilterFields]="['name']"
      dataKey="id"
      selectionMode="multiple"
      [(selection)]="selected"
      #table
    >
      <ng-template #header>
        <tr>
          <th><ui-table-header-checkbox /></th>
          <th uiSortHeader="name">Name</th>
        </tr>
      </ng-template>
      <ng-template #body let-row>
        <tr>
          <td><ui-table-checkbox [value]="row" /></td>
          <td>{{ row.name }}</td>
        </tr>
      </ng-template>
    </ui-data-table>
  `,
})
class HostEngine {
  readonly events: ListLazyLoadEvent[] = [];
  readonly data = signal(rows(57));
  readonly selected = signal<Row[]>([]);
}

/** A table that shows the `{first}`/`{last}` page report — the 2 ELD tables' template. */
@Component({
  selector: "ui-host-report-table",
  imports: [UiDataTable],
  template: `
    <ui-data-table
      [value]="data()"
      [paginator]="true"
      [rows]="10"
      [showCurrentPageReport]="true"
      currentPageReportTemplate="Showing {first} to {last} of {totalRecords} rows"
    >
      <ng-template #body let-row>
        <tr>
          <td>{{ row.name }}</td>
        </tr>
      </ng-template>
    </ui-data-table>
  `,
})
class HostReport {
  readonly data = signal(rows(57));
}

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function bodyRowCount(fixture: ComponentFixture<unknown>): number {
  return (fixture.nativeElement as HTMLElement).querySelectorAll("tbody tr").length;
}

describe("<ui-data-table> engine", () => {
  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  it("a CLIENT-side page-size change actually re-slices the rows", async () => {
    // ⚠ REGRESSION. `pageSize` used to be `computed(() => rows_input() ?? 0)` — it read the INPUT
    // instead of the `size` linkedSignal that `setPageSize()` writes. A LAZY table hid the bug (its
    // store echoes the new size back through `[rows]`), but a CLIENT-paged table has nothing to
    // echo: picking "25" moved the trigger to 25, reset to page 1, and went on rendering 10 rows.
    // Every unit test still passed. The browser caught it.
    const fixture = TestBed.createComponent(HostEngine);
    const table = fixture.debugElement.children[0].componentInstance as UiDataTable<Row>;
    await settle(fixture);
    expect(bodyRowCount(fixture)).toBe(10);

    table["state"].setPageSize(25);
    await settle(fixture);

    expect(bodyRowCount(fixture)).toBe(25);
  });

  it("the client global filter narrows the rows AND resets to the first page", async () => {
    const fixture = TestBed.createComponent(HostEngine);
    const table = fixture.debugElement.children[0].componentInstance as UiDataTable<Row>;
    await settle(fixture);

    // Sit on page 3, then filter down to fewer rows than that page's offset.
    table["state"].setPage(20);
    await settle(fixture);
    expect(bodyRowCount(fixture)).toBe(10);

    table.filterGlobal("Row 42");
    await settle(fixture);

    // Without the `first = 0` reset this renders ZERO rows: one match, but sliced from offset 20.
    expect(bodyRowCount(fixture)).toBe(1);
  });

  it("selection survives a refetch that hands back NEW object instances", async () => {
    // Keyed by `dataKey` into a Set of keys — never by object reference. A refetch replaces every
    // row object, and reference identity would silently drop the whole selection.
    const fixture = TestBed.createComponent(HostEngine);
    const host = fixture.componentInstance;
    await settle(fixture);

    host.selected.set([host.data()[0]]);
    await settle(fixture);

    host.data.set(rows(57)); // brand-new objects, same ids
    await settle(fixture);

    const firstRowCheckbox = (fixture.nativeElement as HTMLElement).querySelector(
      "tbody tr input[type=checkbox]",
    ) as HTMLInputElement;
    expect(firstRowCheckbox.checked).toBe(true);
  });

  it("a fresh sort resets the page offset to 0", async () => {
    const fixture = TestBed.createComponent(HostEngine);
    const table = fixture.debugElement.children[0].componentInstance as UiDataTable<Row>;
    await settle(fixture);

    table["state"].setPage(20);
    await settle(fixture);
    expect(table["state"].first()).toBe(20);

    (fixture.nativeElement as HTMLElement).querySelectorAll<HTMLElement>("thead th")[1].click();
    await settle(fixture);

    expect(table["state"].first()).toBe(0);
    expect(table["state"].sortOrder()).toBe(1);
  });

  it("the page report converts the 0-indexed OFFSET into 1-indexed ROW NUMBERS", async () => {
    // ⚠ `{first}`/`{last}` are 1-indexed ROW NUMBERS; `state.first` is a 0-indexed OFFSET. Reusing
    // the name across both units is a guaranteed off-by-one. On page 3 of a 10-per-page table the
    // offset is 20, so the report must read "21 to 30" — NOT "20 to 30".
    //
    // Nothing pinned this before, and nothing could catch it in the app either: the only two tables
    // that pass this template (the ELD pair) have 5 rows and 0 rows against the seed data, so
    // neither ever renders a second page.
    const fixture = TestBed.createComponent(HostReport);
    const table = fixture.debugElement.children[0].componentInstance as UiDataTable<Row>;
    await settle(fixture);

    const report = (): string =>
      (fixture.nativeElement as HTMLElement).querySelector("ui-table-paginator span")!.textContent!;

    expect(report()).toBe("Showing 1 to 10 of 57 rows");

    table["state"].setPage(20); // page 3
    await settle(fixture);
    expect(report()).toBe("Showing 21 to 30 of 57 rows");

    table["state"].setPage(50); // the last, PARTIAL page: {last} clamps to the total
    await settle(fixture);
    expect(report()).toBe("Showing 51 to 57 of 57 rows");
  });
});
