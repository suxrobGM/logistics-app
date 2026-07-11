/**
 * CHARACTERIZATION SPEC for `<ui-data-table>`.
 *
 * This file is the CONTRACT for the table engine. It passes today against the PrimeNG `<p-table>`
 * internals, and it must keep passing — UNCHANGED — once those internals are hand-rolled.
 *
 * Everything asserted here is behaviour that 82 templates and 33 server-paged list pages depend on
 * SILENTLY. When `primeng` is uninstalled it becomes unrecoverable, so it is written down here.
 *
 * READ THIS BEFORE "FIXING" ANYTHING BELOW. These tests record what the table ACTUALLY DOES today,
 * not what it ought to do. Several recorded behaviours are surprising — the missing `sortOrder` on
 * the initial lazy load, the REPLACE semantics of select-all, the absence of any indeterminate
 * state, the lexicographic ordering of numeric-ish strings. They are surprising AND load-bearing.
 * If a change makes one of these fail, that change altered behaviour production code relies on:
 * fix the change, not the test.
 *
 * ─────────────────────────────────────────────────────────────────────────────────────────────────
 * SELECTOR CONTRACT — the surface a re-implementation must keep so this spec compiles and passes
 * without edits:
 *
 *   • `<ui-data-table>` inputs: value, lazy, paginator, rows, first, totalRecords,
 *     rowsPerPageOptions, sortField, sortOrder, selectionMode, dataKey; two-way `selection`;
 *     output `lazyLoad`.
 *   • Projected `<ng-template #header>` / `#body` slots; `#body` context is `$implicit` + `rowIndex`.
 *   • `<th uiSortHeader="Field">` is clickable and triggers the sort.
 *   • Rows render as `tbody tr`. The paginator renders `<button>`s whose text is the page number,
 *     and a `[role="combobox"]` for the page size whose choices are `[role="option"]`.
 *   • Selection checkboxes render a real `<input type="checkbox">`: one in `thead` (select-all),
 *     one per `tbody tr`.
 *
 * ONE EDIT IS PERMITTED to this file during the swap, and NO others:
 *   1. Renaming the `<p-tableCheckbox>` / `<p-tableHeaderCheckbox>` selectors if the row-directive
 *      module renames them.
 *
 * (A second edit used to be permitted here — deleting a `TableService` workaround provider on
 * `HostSelection`, which existed only because `UiDataTable` did not provide `TableService`. That is
 * DONE: `UiDataTable` provides it now, and the test "provides TableService, so projected row
 * checkboxes construct instead of throwing NG0201" below pins that it keeps doing so.)
 *
 * Every assertion must still hold after that rename.
 * ─────────────────────────────────────────────────────────────────────────────────────────────────
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
  /** Numeric-ish strings — what a "Load #" / "Truck #" column actually holds. */
  code: string;
  customer: { name: string };
}

function rows(n: number): Row[] {
  return Array.from({ length: n }, (_, i) => ({
    id: i + 1,
    name: `Row ${i + 1}`,
    code: String(i + 1),
    customer: { name: `Customer ${i + 1}` },
  }));
}

// ── Test hosts ───────────────────────────────────────────────────────────────────────────────────

/** Exactly the shape the 33 server-paged list pages use: lazy + paginator + sortable headers. */
@Component({
  selector: "ui-host-lazy-table",
  imports: [UiDataTable, UiSortHeader],
  template: `
    <ui-data-table
      [value]="data()"
      [lazy]="true"
      [paginator]="true"
      [rows]="pageSize()"
      [first]="first()"
      [totalRecords]="totalRecords()"
      [rowsPerPageOptions]="[10, 25, 50]"
      (lazyLoad)="events.push($event)"
    >
      <ng-template #header>
        <tr>
          <th uiSortHeader="Name">Name</th>
          <th uiSortHeader="Code">Code</th>
        </tr>
      </ng-template>
      <ng-template #body let-row>
        <tr>
          <td>{{ row.name }}</td>
          <td>{{ row.code }}</td>
        </tr>
      </ng-template>
    </ui-data-table>
  `,
})
class HostLazy {
  readonly events: ListLazyLoadEvent[] = [];
  readonly data = signal(rows(10));
  readonly pageSize = signal(10);
  readonly first = signal(0);
  readonly totalRecords = signal(57);
}

/**
 * Client-side table: no `lazy`, so the table owns sorting and slicing itself.
 *
 * `[first]` and `[totalRecords]` are bound here ON PURPOSE — but no longer out of necessity. The
 * table works without them now: `first` defaults to `0`, and `totalRecords` falls back to
 * `value().length` (production's 14 client-paged templates bind neither, and the ui-lab's client
 * table deliberately binds neither so a regression is visible on screen). They are bound because
 * these fixtures pin the paging ENGINE — slice by `first`/`rows` — and a test of the engine should
 * state its inputs rather than lean on a default it is not the one testing.
 */
@Component({
  selector: "ui-host-client-table",
  imports: [UiDataTable, UiSortHeader],
  template: `
    <ui-data-table
      [value]="data()"
      [paginator]="paginator()"
      [rows]="pageSize()"
      [first]="0"
      [totalRecords]="data().length"
      (lazyLoad)="events.push($event)"
    >
      <ng-template #header>
        <tr>
          <th uiSortHeader="code">Code</th>
          <th uiSortHeader="customer.name">Customer</th>
        </tr>
      </ng-template>
      <ng-template #body let-row>
        <tr>
          <td>{{ row.code }}</td>
          <td>{{ row.customer.name }}</td>
        </tr>
      </ng-template>
    </ui-data-table>
  `,
})
class HostClient {
  readonly events: ListLazyLoadEvent[] = [];
  readonly paginator = signal(false);
  readonly pageSize = signal(2);
  readonly data = signal<Row[]>([]);
}

/**
 * Multi-select with a select-all header checkbox — the bulk-action lists (loads, payroll invoices,
 * attach-load dialog).
 *
 * This host declares NO providers on purpose. The projected `#header` / `#body` templates are
 * declared in the CONSUMER, so everything inside them does DI against the CONSUMER's injector —
 * and `<p-tableCheckbox>` / `<p-tableHeaderCheckbox>` / `[pSelectableRow]` inject BOTH `Table` and
 * `TableService`. `UiDataTable` must therefore re-expose both, and this host is what proves it: if
 * either provider goes missing, every test below dies at construction with NG0201.
 */
@Component({
  selector: "ui-host-selection-table",
  imports: [UiDataTable, UiTableRowDirectives],
  template: `
    <ui-data-table [value]="data()" selectionMode="multiple" dataKey="id" [(selection)]="selected">
      <ng-template #header>
        <tr>
          <th><p-tableHeaderCheckbox /></th>
          <th>Name</th>
        </tr>
      </ng-template>
      <ng-template #body let-row let-i="rowIndex">
        <tr>
          <td><p-tableCheckbox [value]="row" [index]="i" /></td>
          <td>{{ row.name }}</td>
        </tr>
      </ng-template>
    </ui-data-table>
  `,
})
class HostSelection {
  readonly data = signal(rows(3));
  readonly selected = signal<Row[]>([]);
}

// ── Helpers ──────────────────────────────────────────────────────────────────────────────────────

async function settle(fixture: ComponentFixture<unknown>): Promise<void> {
  fixture.detectChanges();
  await fixture.whenStable();
  fixture.detectChanges();
}

function el(fixture: ComponentFixture<unknown>): HTMLElement {
  return fixture.nativeElement as HTMLElement;
}

/** Text of one cell of every rendered body row, in render order. */
function renderedColumn(fixture: ComponentFixture<unknown>, cell = 0): string[] {
  return Array.from(el(fixture).querySelectorAll("tbody tr")).map((tr) =>
    (tr.children[cell]?.textContent ?? "").trim(),
  );
}

function sortHeader(fixture: ComponentFixture<unknown>, index: number): HTMLElement {
  return el(fixture).querySelectorAll("thead th")[index] as HTMLElement;
}

/** The paginator's page-number buttons are the only buttons that carry text. */
function pageButton(fixture: ComponentFixture<unknown>, label: string): HTMLButtonElement {
  const button = Array.from(el(fixture).querySelectorAll("button")).find(
    (b) => (b.textContent ?? "").trim() === label,
  );
  if (!button) {
    throw new Error(`No paginator page button labelled "${label}"`);
  }
  return button as HTMLButtonElement;
}

/** Opens the paginator's page-size control and picks an option by its label. */
async function choosePageSize(fixture: ComponentFixture<unknown>, label: string): Promise<void> {
  const combobox = el(fixture).querySelector("[role=combobox]") as HTMLElement | null;
  if (!combobox) {
    throw new Error("The paginator rendered no rows-per-page combobox");
  }
  combobox.click();
  await settle(fixture);

  const option = Array.from(document.querySelectorAll("[role=option]")).find(
    (o) => (o.textContent ?? "").trim() === label,
  );
  if (!option) {
    throw new Error(`No rows-per-page option labelled "${label}"`);
  }
  (option as HTMLElement).click();
  await settle(fixture);
}

function headerCheckbox(fixture: ComponentFixture<unknown>): HTMLInputElement {
  return el(fixture).querySelector("thead input[type=checkbox]") as HTMLInputElement;
}

function rowCheckboxes(fixture: ComponentFixture<unknown>): HTMLInputElement[] {
  return Array.from(el(fixture).querySelectorAll("tbody tr input[type=checkbox]"));
}

describe("<ui-data-table> — the table engine contract", () => {
  beforeAll(() => {
    // Test-environment gap, not table behaviour: the paginator's page-size overlay reads
    // `document.defaultView.matchMedia`, which the DOM shim does not implement.
    if (typeof window.matchMedia !== "function") {
      const noop = (): undefined => undefined;
      window.matchMedia = ((query: string) => ({
        matches: false,
        media: query,
        onchange: null,
        addListener: noop,
        removeListener: noop,
        addEventListener: noop,
        removeEventListener: noop,
        dispatchEvent: () => false,
      })) as unknown as typeof window.matchMedia;
    }
  });

  beforeEach(() => {
    TestBed.configureTestingModule({ providers: [provideZonelessChangeDetection()] });
  });

  // ── The initial lazy load ─────────────────────────────────────────────────────────────────────
  // The only reason a server-paged page ever shows data: nothing else asks for the first page.
  // Zero emits reads as "no data"; two emits doubles every request on all 33 lists.

  describe("initial load", () => {
    it("a lazy table asks for its first page EXACTLY ONCE on mount", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(1);
    });

    it("the initial lazy request carries first = 0 and the bound page size", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      const event = fixture.componentInstance.events[0];
      expect(event.first).toBe(0);
      expect(event.rows).toBe(10);
    });

    it("the initial lazy request carries NO sort at all — sortOrder is undefined, not 1", async () => {
      // ⚠ THIS CONTRADICTS THE FOLKLORE. `<p-table>`'s own field default for sortOrder is 1, but
      // `<ui-data-table>` forwards its own `sortOrder` input unconditionally and that input
      // defaults to `undefined`. Angular writes `undefined` into the table on the first
      // change-detection pass, clobbering the 1 BEFORE ngOnInit fires the initial lazy load. No
      // list page binds `sortOrder`, so in production the first request of every list goes out
      // with sortOrder = undefined.
      //
      // This is why `createListStore.onLazyLoad` does `event.sortOrder ?? -1`. That fallback is
      // LIVE on every list's first request — it is not dead code, and it makes the initial sort
      // DESCENDING. A re-implementation that "helpfully" emits 1 here would silently flip the
      // default ordering of all 33 lists.
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      const event = fixture.componentInstance.events[0];
      expect(event.sortField).toBeUndefined();
      expect(event.sortOrder).toBeUndefined();
    });

    it("a non-lazy table NEVER asks for data", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.data.set(rows(5));
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(0);
    });
  });

  // ── Sorting ───────────────────────────────────────────────────────────────────────────────────
  // ~50 sort headers depend on this exact sequence.

  describe("sorting a lazy table", () => {
    it("clicking a fresh column sorts ASCENDING (sortOrder = 1) in a single request", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);
      fixture.componentInstance.events.length = 0;

      sortHeader(fixture, 0).click();
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(1);
      expect(fixture.componentInstance.events[0].sortField).toBe("Name");
      expect(fixture.componentInstance.events[0].sortOrder).toBe(1);
    });

    it("clicking the SAME column again toggles to descending (sortOrder = -1)", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      sortHeader(fixture, 0).click();
      await settle(fixture);
      fixture.componentInstance.events.length = 0;

      sortHeader(fixture, 0).click();
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(1);
      expect(fixture.componentInstance.events[0].sortField).toBe("Name");
      expect(fixture.componentInstance.events[0].sortOrder).toBe(-1);
    });

    it("moving to a DIFFERENT column restarts at ascending — it does not inherit the old order", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      sortHeader(fixture, 0).click(); // Name asc
      await settle(fixture);
      sortHeader(fixture, 0).click(); // Name desc
      await settle(fixture);
      fixture.componentInstance.events.length = 0;

      sortHeader(fixture, 1).click(); // Code — a fresh column
      await settle(fixture);

      expect(fixture.componentInstance.events[0].sortField).toBe("Code");
      expect(fixture.componentInstance.events[0].sortOrder).toBe(1);
    });

    it("sorting from page 3 jumps back to the first row AND still issues only ONE request", async () => {
      // The page reset and the sort are coalesced into a single lazy load. An implementation that
      // emitted one event for "first = 0" and another for "sort changed" would double every
      // request on every list.
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      pageButton(fixture, "3").click();
      await settle(fixture);
      expect(fixture.componentInstance.events.at(-1)?.first).toBe(20);
      fixture.componentInstance.events.length = 0;

      sortHeader(fixture, 0).click();
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(1);
      expect(fixture.componentInstance.events[0].first).toBe(0);
      expect(fixture.componentInstance.events[0].sortField).toBe("Name");
    });

    it("the sorted column reports its direction through aria-sort", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);
      expect(sortHeader(fixture, 0).getAttribute("aria-sort")).toBe("none");

      sortHeader(fixture, 0).click();
      await settle(fixture);
      expect(sortHeader(fixture, 0).getAttribute("aria-sort")).toBe("ascending");

      sortHeader(fixture, 0).click();
      await settle(fixture);
      expect(sortHeader(fixture, 0).getAttribute("aria-sort")).toBe("descending");
    });
  });

  // ── Paging ────────────────────────────────────────────────────────────────────────────────────

  describe("paging a lazy table", () => {
    it("jumping to page N requests first = rows * (N - 1), keeps the page size, and emits ONCE", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);
      fixture.componentInstance.events.length = 0;

      pageButton(fixture, "3").click();
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(1);
      expect(fixture.componentInstance.events[0].first).toBe(20);
      expect(fixture.componentInstance.events[0].rows).toBe(10);
    });

    it("changing the page size emits ONE request, back at the first row", async () => {
      const fixture = TestBed.createComponent(HostLazy);
      await settle(fixture);

      pageButton(fixture, "3").click(); // first = 20
      await settle(fixture);
      fixture.componentInstance.events.length = 0;

      await choosePageSize(fixture, "25");

      expect(fixture.componentInstance.events.length).toBe(1);
      expect(fixture.componentInstance.events[0].rows).toBe(25);
      expect(fixture.componentInstance.events[0].first).toBe(0);
    });
  });

  // ── Selection ─────────────────────────────────────────────────────────────────────────────────

  describe("selection", () => {
    it("provides TableService, so projected row checkboxes construct instead of throwing NG0201", async () => {
      // ⚠ THE OUTAGE TEST. `<ui-data-table>` must re-provide BOTH `Table` and `TableService` on its
      // own host node. The projected `#header` / `#body` templates are declared in the consumer, so
      // the row directives inside them resolve DI against the CONSUMER's injector, where neither is
      // visible: `TableService` is `@Injectable()` with no `providedIn` and lives only in `Table`'s
      // node providers. Drop the `TableService` provider and `TableCheckbox` /
      // `TableHeaderCheckbox` / `SelectableRow` throw NG0201 at construction — which does not just
      // blank the checkbox, it takes the WHOLE table's render down. `/loads` shipped exactly that:
      // a summary card reading "195 Total Loads" above an empty table reading "0 of 0".
      //
      // So this asserts on the DOM, not on the injector: the checkboxes must really be there, and
      // the rows around them must really have rendered.
      const fixture = TestBed.createComponent(HostSelection);
      await settle(fixture); // Throws NG0201 here if either provider is missing.

      // The select-all checkbox and one checkbox per row are real, rendered inputs…
      expect(headerCheckbox(fixture)).toBeTruthy();
      expect(rowCheckboxes(fixture).length).toBe(3);

      // …and the body rendered its rows rather than collapsing to an empty table.
      expect(renderedColumn(fixture, 1)).toEqual(["Row 1", "Row 2", "Row 3"]);
    });

    it("the select-all checkbox REPLACES the selection with exactly the table's rows", async () => {
      // ⚠ REPLACE, not merge. `selectionPageOnly` is set nowhere in this repo, so select-all starts
      // from an EMPTY array and appends the whole `value`. Bulk actions rely on getting exactly the
      // current `value` back.
      const fixture = TestBed.createComponent(HostSelection);
      const host = fixture.componentInstance;
      await settle(fixture);

      headerCheckbox(fixture).click();
      await settle(fixture);

      expect(host.selected()).toEqual(host.data());
      expect(host.selected().length).toBe(3);
    });

    it("select-all DISCARDS anything already selected that is not in `value`", async () => {
      // The corollary of REPLACE: a row selected on another page does not survive select-all.
      const fixture = TestBed.createComponent(HostSelection);
      const host = fixture.componentInstance;
      host.selected.set([{ id: 99, name: "Off page", code: "99", customer: { name: "Ghost" } }]);
      await settle(fixture);

      headerCheckbox(fixture).click();
      await settle(fixture);

      expect(host.selected()).toEqual(host.data());
      expect(host.selected().some((r) => r.id === 99)).toBe(false);
    });

    it("unchecking the select-all checkbox empties the selection", async () => {
      const fixture = TestBed.createComponent(HostSelection);
      const host = fixture.componentInstance;
      await settle(fixture);

      headerCheckbox(fixture).click();
      await settle(fixture);
      expect(host.selected().length).toBe(3);

      headerCheckbox(fixture).click();
      await settle(fixture);

      expect(host.selected()).toEqual([]);
    });

    it("a PARTIAL selection renders the select-all checkbox UNCHECKED — there is no tri-state", async () => {
      // ⚠ COUNTER-INTUITIVE AND DELIBERATE. The select-all checkbox is plain binary: checked only
      // when EVERY row is selected, otherwise unchecked. It has no indeterminate/mixed rendering
      // today and nothing depends on one. Someone WILL want to "fix" this while re-implementing;
      // adding a tri-state is a behaviour change, not a bug fix.
      const fixture = TestBed.createComponent(HostSelection);
      const host = fixture.componentInstance;
      host.selected.set([host.data()[0]]);
      await settle(fixture);

      // The selection really is partial — row 0 is checked, row 1 is not…
      expect(rowCheckboxes(fixture)[0].checked).toBe(true);
      expect(rowCheckboxes(fixture)[1].checked).toBe(false);

      // …and the header checkbox is simply off.
      const header = headerCheckbox(fixture);
      expect(header.checked).toBe(false);
      expect(header.indeterminate).toBe(false);
      expect(header.getAttribute("aria-checked")).not.toBe("mixed");
    });

    it("row checkboxes ADD to the selection one row at a time — they never replace it", async () => {
      const fixture = TestBed.createComponent(HostSelection);
      const host = fixture.componentInstance;
      await settle(fixture);

      rowCheckboxes(fixture)[0].click();
      await settle(fixture);
      expect(host.selected()).toEqual([host.data()[0]]);

      rowCheckboxes(fixture)[2].click();
      await settle(fixture);
      expect(host.selected()).toEqual([host.data()[0], host.data()[2]]);

      // …and toggling one back off removes only that row.
      rowCheckboxes(fixture)[0].click();
      await settle(fixture);
      expect(host.selected()).toEqual([host.data()[2]]);
    });
  });

  // ── The client-side engine (lazy = false) ─────────────────────────────────────────────────────
  // Not every table is server-paged; the client-side ones need a real sort + slice engine.

  describe("client-side sorting", () => {
    it("sorting reorders the RENDERED rows, and clicking again reverses them", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.data.set([
        { id: 1, name: "b", code: "b", customer: { name: "Zeta" } },
        { id: 2, name: "a", code: "a", customer: { name: "Alpha" } },
        { id: 3, name: "c", code: "c", customer: { name: "Mid" } },
      ]);
      await settle(fixture);

      sortHeader(fixture, 0).click(); // ascending
      await settle(fixture);
      expect(renderedColumn(fixture)).toEqual(["a", "b", "c"]);

      sortHeader(fixture, 0).click(); // descending
      await settle(fixture);
      expect(renderedColumn(fixture)).toEqual(["c", "b", "a"]);
    });

    it("sorting resolves a DOTTED field path (customer.name), not just a top-level key", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.data.set([
        { id: 1, name: "x", code: "x", customer: { name: "Zeta" } },
        { id: 2, name: "y", code: "y", customer: { name: "Alpha" } },
        { id: 3, name: "z", code: "z", customer: { name: "Mid" } },
      ]);
      await settle(fixture);

      sortHeader(fixture, 1).click();
      await settle(fixture);

      expect(renderedColumn(fixture, 1)).toEqual(["Alpha", "Mid", "Zeta"]);
    });

    it("sorts numeric-ish STRINGS lexicographically, so '10' and '100' come BEFORE '9'", async () => {
      // ⚠ CHARACTERIZATION OF A WART, recorded because it ships today. String cells are compared
      // with `localeCompare`, which is not numeric-aware, so a "Load #" column of strings orders
      // 10, 100, 9 — not 9, 10, 100. Making the comparison numeric-aware would visibly change every
      // client-sorted string column, so it is a separate decision, not something to slip in while
      // swapping the engine.
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.data.set([
        { id: 1, name: "n", code: "9", customer: { name: "C1" } },
        { id: 2, name: "n", code: "100", customer: { name: "C2" } },
        { id: 3, name: "n", code: "10", customer: { name: "C3" } },
      ]);
      await settle(fixture);

      sortHeader(fixture, 0).click();
      await settle(fixture);

      expect(renderedColumn(fixture)).toEqual(["10", "100", "9"]);
    });

    it("sorts real NUMBERS numerically — the wart above is specific to strings", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.data.set([
        { id: 1, name: "n", code: 9 as unknown as string, customer: { name: "C1" } },
        { id: 2, name: "n", code: 100 as unknown as string, customer: { name: "C2" } },
        { id: 3, name: "n", code: 10 as unknown as string, customer: { name: "C3" } },
      ]);
      await settle(fixture);

      sortHeader(fixture, 0).click();
      await settle(fixture);

      expect(renderedColumn(fixture)).toEqual(["9", "10", "100"]);
    });
  });

  describe("client-side paging", () => {
    // ⚠ THE HOST BINDS `[first]` AND `[totalRecords]`, AND IT HAS TO.
    //
    // `<ui-data-table>` forwards `first`, `totalRecords` and `sortOrder` unconditionally, and all
    // three inputs default to `undefined`. Writing `undefined` into the table clobbers the table's
    // own field defaults (0, 0 and 1). With `first === undefined` the page slice degenerates to
    // `slice(undefined, NaN)` → NO ROWS, and with `totalRecords === undefined` the page count is 0
    // → NO PAGE BUTTONS.
    //
    // Bind both and the engine below works. All 14 client-paged templates in the app bind NEITHER,
    // which is a live defect — see the report accompanying this spec. These tests pin the ENGINE
    // (slice by first/rows, page N ⇒ first = rows·(N−1)); they deliberately do NOT pin the defect.

    it("renders only `rows` rows per page and advances a page at a time", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.paginator.set(true);
      fixture.componentInstance.pageSize.set(2);
      fixture.componentInstance.data.set(rows(5));
      await settle(fixture);

      expect(renderedColumn(fixture)).toEqual(["1", "2"]);

      pageButton(fixture, "2").click();
      await settle(fixture);
      expect(renderedColumn(fixture)).toEqual(["3", "4"]);

      pageButton(fixture, "3").click();
      await settle(fixture);
      expect(renderedColumn(fixture)).toEqual(["5"]);
    });

    it("paging client-side still emits NO lazyLoad", async () => {
      const fixture = TestBed.createComponent(HostClient);
      fixture.componentInstance.paginator.set(true);
      fixture.componentInstance.data.set(rows(5));
      await settle(fixture);

      pageButton(fixture, "2").click();
      await settle(fixture);

      expect(fixture.componentInstance.events.length).toBe(0);
    });
  });
});
