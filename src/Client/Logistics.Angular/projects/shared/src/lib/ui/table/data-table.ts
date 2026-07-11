import { NgTemplateOutlet } from "@angular/common";
import {
  booleanAttribute,
  Component,
  contentChild,
  forwardRef,
  input,
  model,
  output,
  viewChild,
  type TemplateRef,
} from "@angular/core";
import { Table, TableModule, TableService } from "primeng/table";
import type { ListLazyLoadEvent } from "../../stores";

export type DataTableSize = "small" | "large" | undefined;
export type DataTableSelectionMode = "single" | "multiple" | undefined;

/**
 * The one place that knows what renders a data table.
 *
 * Feature templates project the familiar `#header` / `#body` / `#footer` / `#empty` / `#caption`
 * slots and use `<th uiSortHeader="Field">` for sortable columns, so no feature code imports
 * `primeng/table`. Swapping the implementation is then a change to this component and
 * `UiSortHeader`, not to the 82 templates that use them.
 *
 * The input surface is deliberately only what the codebase actually uses. Column filters, cell
 * editing, frozen/resizable/reorderable columns, virtual scroll and row grouping are used
 * nowhere, so they are not exposed.
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
  imports: [TableModule, NgTemplateOutlet],
  providers: [
    // The projected `#header` / `#body` templates are declared in the CONSUMER, so anything
    // inside them (e.g. `<th uiSortHeader>`, `<p-sortIcon>`) resolves DI against the consumer's
    // injector, not the inner p-table's — it would fail with NG0201. Re-expose the inner Table
    // here. The factory is lazy: it runs on first injection, by which point the view query has
    // resolved.
    {
      provide: Table,
      useFactory: (host: UiDataTable<unknown>) => host.innerTable(),
      deps: [forwardRef(() => UiDataTable)],
    },
    // `Table` alone is not enough. `TableCheckbox`, `TableHeaderCheckbox` and `SelectableRow`
    // (re-exported through `UiTableRowDirectives`) inject BOTH `Table` and `TableService`, and
    // `TableService` is `@Injectable()` with no `providedIn` — it lives only in `Table`'s own node
    // providers, which the consumer's injector cannot see. Without this, every projected checkbox
    // throws NG0201 and takes the whole table's render down with it: `/loads` rendered a summary
    // card saying "195 loads" above an empty grey table reading "0 of 0".
    {
      provide: TableService,
      useFactory: (host: UiDataTable<unknown>) => host.innerTable().tableService,
      deps: [forwardRef(() => UiDataTable)],
    },
  ],
})
export class UiDataTable<T> {
  /** @internal exposed only so the `Table` provider above can alias it. */
  public readonly innerTable = viewChild.required(Table);

  public readonly value = input.required<readonly T[]>();

  // Server-side paging / sorting
  public readonly lazy = input(false, { transform: booleanAttribute });
  public readonly paginator = input(false, { transform: booleanAttribute });
  public readonly rows = input<number | undefined>(undefined);
  /**
   * 0-indexed row offset of the current page.
   *
   * Must default to PrimeNG's own default (`0`), not `undefined`. An unbound `input()` forwards
   * `undefined` into `<p-table>` on the first change-detection pass, overwriting its `_first = 0`
   * before `ngOnInit` runs — after which `dataToRender()` computes `slice(undefined, NaN)` and
   * every client-paginated table renders **zero rows**. Same bug class as
   * `currentPageReportTemplate` below; see {@link totalRecords}.
   */
  public readonly first = input<number>(0);
  /**
   * Total row count. Server-lazy tables bind it; client-side tables leave it unbound, in which case
   * it falls back to `value().length` — which is exactly what PrimeNG does internally when its own
   * `_totalRecords` is 0. Forwarding a bare `undefined` instead makes the paginator report "0 of 0"
   * and render no page links.
   */
  public readonly totalRecords = input<number | undefined>(undefined);
  public readonly rowsPerPageOptions = input<number[] | undefined>(undefined);
  public readonly showCurrentPageReport = input(false, { transform: booleanAttribute });
  public readonly loading = input(false, { transform: booleanAttribute });

  // Sorting
  public readonly sortField = input<string | undefined>(undefined);
  public readonly sortOrder = input<number | undefined>(undefined);
  public readonly sortMode = input<"single" | "multiple">("single");

  // Selection
  public readonly dataKey = input<string | undefined>(undefined);
  public readonly selectionMode = input<DataTableSelectionMode>(undefined);
  /**
   * Selected row(s). Untyped on purpose: the shape depends on `selectionMode` (a row, or an
   * array of rows), exactly as PrimeNG models it.
   */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  public readonly selection = model<any>(undefined);

  // Presentation
  public readonly scrollable = input(false, { transform: booleanAttribute });
  public readonly scrollHeight = input<string | undefined>(undefined);
  public readonly styleClass = input<string | undefined>(undefined);
  public readonly tableStyle = input<Record<string, string> | undefined>(undefined);
  public readonly size = input<DataTableSize>(undefined);
  public readonly stripedRows = input(false, { transform: booleanAttribute });
  public readonly rowHover = input(false, { transform: booleanAttribute });
  public readonly showLoader = input(true, { transform: booleanAttribute });
  /**
   * Must default to PrimeNG's own default rather than `undefined`: the paginator does
   * `currentPageReportTemplate.replace(...)` unguarded, so forwarding `undefined` throws.
   */
  public readonly currentPageReportTemplate = input<string>("{currentPage} of {totalPages}");
  public readonly globalFilterFields = input<string[] | undefined>(undefined);

  /** Emitted when the table needs a page of data (paging or sorting changed). */
  public readonly lazyLoad = output<ListLazyLoadEvent>();

  /**
   * Applies the table's client-side global filter. Only used by the trip wizard's load picker;
   * every other list filters server-side through its store.
   */
  public filterGlobal(value: string, matchMode = "contains"): void {
    this.innerTable().filterGlobal(value, matchMode);
  }

  // Projected slots. Rendered through ngTemplateOutlet, so an absent slot renders nothing.
  protected readonly headerTpl = contentChild<TemplateRef<unknown>>("header");
  protected readonly bodyTpl = contentChild<TemplateRef<unknown>>("body");
  protected readonly footerTpl = contentChild<TemplateRef<unknown>>("footer");
  protected readonly emptyTpl = contentChild<TemplateRef<unknown>>("empty");
  protected readonly captionTpl = contentChild<TemplateRef<unknown>>("caption");
  protected readonly expandedRowTpl = contentChild<TemplateRef<unknown>>("expandedrow");
}
