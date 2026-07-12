import { NgTemplateOutlet } from "@angular/common";
import {
  booleanAttribute,
  Component,
  computed,
  contentChild,
  inject,
  input,
  model,
  output,
  type OnInit,
  type TemplateRef,
} from "@angular/core";
import type { ListLazyLoadEvent } from "../../stores";
import { HlmTable, HlmTBody, HlmTFoot, HlmTHead } from "../primitives/table";
import { Spinner } from "../status/spinner/spinner";
import { UiTablePaginator } from "./table-paginator";
import { resolveFieldData, UiTableState, type UiTableSelectionMode } from "./table-state";

export type DataTableSize = "small" | "large" | undefined;
export type DataTableSelectionMode = UiTableSelectionMode;

/**
 * Data table. Feature templates project the `#header` / `#body` / `#footer` / `#empty` /
 * `#caption` / `#expandedrow` slots and use `<th uiSortHeader="Field">` for sortable columns.
 *
 * The markup is Helm's presentational table directives; all behaviour lives in {@link UiTableState},
 * provided on this component's own node so that a `<th uiSortHeader>` or `<ui-table-checkbox>`
 * written inside a CONSUMER's `<ng-template>` can inject it.
 *
 * The input surface is deliberately only what the codebase uses — no column filters, cell editing,
 * frozen/resizable columns, virtual scroll or row grouping.
 *
 * @example
 * <ui-data-table
 *   [value]="store.data()"
 *   [lazy]="true"
 *   [paginator]="true"
 *   [rows]="store.pageSize()"
 *   [first]="store.first()"
 *   [totalRecords]="store.totalRecords()"
 *   [loading]="store.isLoading()"
 *   [rowsPerPageOptions]="[10, 25, 50]"
 *   (lazyLoad)="store.onLazyLoad($event)"
 * >
 *   <ng-template #header>
 *     <tr><th uiSortHeader="Name">Name</th><th>Status</th></tr>
 *   </ng-template>
 *   <ng-template #body let-customer>
 *     <tr><td>{{ customer.name }}</td><td>{{ customer.status }}</td></tr>
 *   </ng-template>
 * </ui-data-table>
 */
@Component({
  selector: "ui-data-table",
  templateUrl: "./data-table.html",
  imports: [NgTemplateOutlet, HlmTable, HlmTHead, HlmTBody, HlmTFoot, UiTablePaginator, Spinner],
  providers: [UiTableState],
})
export class UiDataTable<T> implements OnInit {
  protected readonly state = inject(UiTableState) as UiTableState<T>;

  public readonly value = input.required<readonly T[]>();

  // Server-side paging / sorting
  public readonly lazy = input(false, { transform: booleanAttribute });
  public readonly paginator = input(false, { transform: booleanAttribute });
  public readonly rows = input<number | undefined>(undefined);
  /**
   * 0-indexed row offset of the current page — not a page number.
   *
   * Must default to `0`, never `undefined`: most client-paged consumers leave it unbound, and an
   * `undefined` reaching the row slice degenerates to `slice(undefined, NaN)`, rendering no rows.
   */
  public readonly first = input<number>(0);
  /**
   * Total row count. Server-lazy tables bind it; client-side tables leave it unbound and the engine
   * counts the rows that survived the filter instead of reporting "0 of 0".
   */
  public readonly totalRecords = input<number | undefined>(undefined);
  /**
   * Page-size choices. Not hardcoded: tables that bind nothing must show no page-size control at
   * all, and `UiTablePaginator` renders the dropdown only when this is bound.
   */
  public readonly rowsPerPageOptions = input<number[] | undefined>(undefined);
  public readonly showCurrentPageReport = input(false, { transform: booleanAttribute });
  public readonly loading = input(false, { transform: booleanAttribute });

  // Sorting
  public readonly sortField = input<string | undefined>(undefined);
  public readonly sortOrder = input<number | undefined>(undefined);

  // Selection
  public readonly dataKey = input<string | undefined>(undefined);
  public readonly selectionMode = input<DataTableSelectionMode>(undefined);
  /** Selected row(s). Untyped on purpose: a row, or an array of rows, per `selectionMode`. */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public readonly selection = model<any>(undefined);

  // Presentation
  public readonly scrollable = input(false, { transform: booleanAttribute });
  /** A CSS length, or the literal `"flex"` to fill the available height of a flex parent. */
  public readonly scrollHeight = input<string | undefined>(undefined);
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly tableStyle = input<Record<string, string> | undefined>(undefined);
  public readonly size = input<DataTableSize>(undefined);
  public readonly stripedRows = input(false, { transform: booleanAttribute });
  public readonly rowHover = input(false, { transform: booleanAttribute });
  public readonly showLoader = input(true, { transform: booleanAttribute });
  public readonly currentPageReportTemplate = input<string>("{currentPage} of {totalPages}");
  public readonly globalFilterFields = input<string[] | undefined>(undefined);

  /** Emitted when the table needs a page of data (paging or sorting changed). */
  public readonly lazyLoad = output<ListLazyLoadEvent>();

  constructor() {
    this.state.connect(
      {
        value: this.value,
        lazy: this.lazy,
        paginator: this.paginator,
        rows: this.rows,
        first: this.first,
        totalRecords: this.totalRecords,
        sortField: this.sortField,
        sortOrder: this.sortOrder,
        dataKey: this.dataKey,
        selectionMode: this.selectionMode,
        selection: this.selection,
        globalFilterFields: this.globalFilterFields,
      },
      () => this.emitLazy(),
    );
  }

  /**
   * The initial page request — the only reason a server-paged page ever shows data.
   *
   * Imperative, once, synchronously. Never derive this from an `effect()` over the state signals:
   * that yields either zero emits (reads as "no data") or two (doubles every list request). Every
   * later emit comes from a user-intent method on `UiTableState`, each calling {@link emitLazy}
   * exactly once; inputs arriving later re-seed the state's `linkedSignal`s and must not emit.
   */
  public ngOnInit(): void {
    if (this.lazy()) {
      this.emitLazy();
    }
  }

  /** The only `.emit()` call site for `lazyLoad`. */
  private emitLazy(): void {
    this.lazyLoad.emit({
      first: this.state.first(),
      rows: this.state.size(),
      sortField: this.state.sortField() || undefined,
      // `undefined`, not 1, until something actually sorts. See UiTableState.sortOrder.
      sortOrder: this.state.sortOrder(),
    });
  }

  /**
   * Applies the client-side global filter. `matchMode` is accepted for call-site compatibility:
   * "contains" is the only mode ever passed and the only one implemented.
   */
  public filterGlobal(value: string, matchMode = "contains"): void {
    void matchMode;
    this.state.filterGlobal(value);
  }

  // Projected slots. Rendered through ngTemplateOutlet, so an absent slot renders nothing.
  protected readonly headerTpl = contentChild<TemplateRef<unknown>>("header");
  protected readonly bodyTpl = contentChild<TemplateRef<unknown>>("body");
  protected readonly footerTpl = contentChild<TemplateRef<unknown>>("footer");
  protected readonly emptyTpl = contentChild<TemplateRef<unknown>>("empty");
  protected readonly captionTpl = contentChild<TemplateRef<unknown>>("caption");
  protected readonly expandedRowTpl = contentChild<TemplateRef<unknown>>("expandedrow");

  protected readonly rowsToRender = computed(() => this.state.rowsToRender());

  /** A `"flex"` scroll height fills the flex parent; anything else is a max-height. */
  protected readonly maxHeight = computed(() => {
    const height = this.scrollHeight();
    return !height || height === "flex" ? null : height;
  });

  protected readonly cellPadding = computed(() => {
    switch (this.size()) {
      case "small":
        return "[&>tr>td]:px-2 [&>tr>td]:py-1.5";
      case "large":
        return "[&>tr>td]:px-4 [&>tr>td]:py-4";
      default:
        return "[&>tr>td]:px-3 [&>tr>td]:py-2.5";
    }
  });

  /**
   * `[&>tr>td]`, not `[&_td]`: a descendant selector on `<tbody>` reaches into a nested
   * `<ui-data-table>` rendered inside an expanded row and restyles its cells too.
   */
  protected readonly bodyClass = computed(() =>
    [
      "[&>tr]:border-border [&>tr]:border-b [&>tr>td]:align-middle",
      this.cellPadding(),
      this.stripedRows() ? "[&>tr:nth-child(even)]:bg-muted/40" : "",
      this.rowHover() ? "[&>tr:hover]:bg-muted/50" : "",
    ]
      .filter(Boolean)
      .join(" "),
  );

  protected trackRow(index: number, row: T): unknown {
    const dataKey = this.dataKey();
    if (!dataKey) {
      return index;
    }
    return resolveFieldData(row, dataKey) ?? index;
  }
}
