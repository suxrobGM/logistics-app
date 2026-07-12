import {
  computed,
  Injectable,
  isDevMode,
  linkedSignal,
  signal,
  type Signal,
  type WritableSignal,
} from "@angular/core";

export type UiTableSelectionMode = "single" | "multiple" | undefined;

/**
 * The input signals `<ui-data-table>` hands to its state on construction.
 *
 * Passed in rather than injected: `UiDataTable` provides `UiTableState` on its own node, so
 * `inject(UiDataTable)` from inside the state would be a construction cycle (NG0200).
 */
export interface UiTableSources<T> {
  value: Signal<readonly T[]>;
  lazy: Signal<boolean>;
  paginator: Signal<boolean>;
  rows: Signal<number | undefined>;
  first: Signal<number>;
  totalRecords: Signal<number | undefined>;
  sortField: Signal<string | undefined>;
  sortOrder: Signal<number | undefined>;
  dataKey: Signal<string | undefined>;
  selectionMode: Signal<UiTableSelectionMode>;
  /** Untyped for the same reason `UiDataTable.selection` is: a row, or an array of rows. */
  // eslint-disable-next-line @typescript-eslint/no-explicit-any
  selection: WritableSignal<any>;
  globalFilterFields: Signal<string[] | undefined>;
}

/** Resolves a possibly-dotted field path ("customer.name", "originAddress.line1") against a row. */
export function resolveFieldData(data: unknown, field: string | undefined): unknown {
  if (data == null || !field) {
    return undefined;
  }
  if (!field.includes(".")) {
    return (data as Record<string, unknown>)[field];
  }
  let value: unknown = data;
  for (const part of field.split(".")) {
    if (value == null) {
      return undefined;
    }
    value = (value as Record<string, unknown>)[part];
  }
  return value;
}

/**
 * Nullish sorts first on ascending (and therefore last on descending).
 *
 * Strings compare with `localeCompare`, which is not numeric-aware, so a column of numeric-ish
 * strings ("Load #") orders "10", "100", "9". Do not "fix" it here — client-sorted templates depend
 * on today's ordering, and it is pinned by the contract spec. Real numbers sort numerically.
 */
function compareFieldValues(value1: unknown, value2: unknown): number {
  if (value1 == null && value2 != null) {
    return -1;
  }
  if (value1 != null && value2 == null) {
    return 1;
  }
  if (value1 == null && value2 == null) {
    return 0;
  }
  if (typeof value1 === "string" && typeof value2 === "string") {
    return value1.localeCompare(value2);
  }
  return (value1 as number) < (value2 as number)
    ? -1
    : (value1 as number) > (value2 as number)
      ? 1
      : 0;
}

/** The "contains" match mode: accent-insensitive, case-insensitive substring. */
function contains(value: unknown, filter: string): boolean {
  if (filter.trim() === "") {
    return true;
  }
  if (value == null) {
    return false;
  }
  const normalize = (input: string): string =>
    input.normalize("NFD").replace(/[̀-ͯ]/g, "").toLocaleLowerCase();
  return normalize(String(value)).includes(normalize(filter));
}

/**
 * The table engine: client sort, client paging, client global filter, selection, row expansion, the
 * sort/page state `<th uiSortHeader>` reads, and the lazy-load request server-paged pages live on.
 *
 * Provided on `<ui-data-table>`'s own node, which is what lets a `<th uiSortHeader>` written in a
 * CONSUMER's `<ng-template #header>` inject it: the template is declared as a child of the
 * `<ui-data-table>` element, and `createEmbeddedView` parents the embedded view's injector at the
 * declaration node. A nested table's inner state therefore shadows the outer one structurally.
 *
 * Nothing here emits. This class calls the `emit` callback handed to {@link connect}, and only from
 * a user-intent method; the `lazyLoad` output itself belongs to `UiDataTable`.
 */
@Injectable()
export class UiTableState<T = unknown> {
  private src!: UiTableSources<T>;
  private emit: () => void = () => undefined;

  // State: `linkedSignal`s seeded from the inputs. Seeding never emits, so a consumer's `[first]`
  // or `sortField` is already in state before `ngOnInit` fires the initial lazy load, and a later
  // input change (a store writing `first` back) re-seeds without emitting either.

  /** 0-indexed row offset of the current page. Not a page number. */
  public readonly first = linkedSignal(() => this.src.first());
  /** Page size. `undefined` when the consumer binds no `[rows]`. */
  public readonly size = linkedSignal(() => this.src.rows());
  public readonly sortField = linkedSignal(() => this.src.sortField());
  /**
   * `undefined` until something sorts — not 1. The initial lazy request must carry
   * `sortOrder: undefined` so `createListStore` applies its own `?? -1`; emitting a nominal `1`
   * would silently change what the store records having asked for. Pinned by the contract spec.
   */
  public readonly sortOrder = linkedSignal(() => this.src.sortOrder());
  private readonly globalFilter = signal("");
  private readonly expandedKeys = signal<ReadonlySet<unknown>>(new Set());
  private readonly warnedSortFields = new Set<string>();

  public connect(sources: UiTableSources<T>, emit: () => void): void {
    this.src = sources;
    this.emit = emit;
  }

  public readonly lazy = computed(() => this.src.lazy());
  public readonly paginator = computed(() => this.src.paginator());
  public readonly selectionMode = computed(() => this.src.selectionMode());
  /**
   * Page size, 0 when unbound; every slice/page computation guards on it.
   *
   * Reads {@link size}, the linkedSignal — not the raw `rows` input. `setPageSize()` writes `size`,
   * so reading the input here would bypass it and a client-paged table (which has no store to write
   * the new size back through `[rows]`) would keep rendering the old page size forever.
   */
  public readonly pageSize = computed(() => this.size() ?? 0);

  /**
   * A client sort needs a direction even though `sortOrder` stays `undefined` on the wire: a
   * bound-but-orderless sort field means ascending, and the header toggles from there.
   */
  private readonly effectiveSortOrder = computed(() => this.sortOrder() ?? 1);

  // The client pipeline: filter -> sort -> slice. For a lazy table `value` IS the server's page, so
  // none of the three steps apply.

  private readonly filtered = computed<readonly T[]>(() => {
    const data = this.src.value();
    const filter = this.globalFilter();
    const fields = this.src.globalFilterFields();
    if (this.lazy() || !filter || !fields?.length) {
      return data;
    }
    return data.filter((row) =>
      fields.some((field) => contains(resolveFieldData(row, field), filter)),
    );
  });

  /**
   * Filtered and sorted, but not paged — this is what select-all and the header checkbox operate
   * on, so select-all takes every matching row rather than just the visible page.
   */
  public readonly processed = computed<readonly T[]>(() => {
    const data = this.filtered();
    const field = this.sortField();
    if (this.lazy() || !field) {
      return data;
    }
    const order = this.effectiveSortOrder();
    // Copy, not an in-place sort: never mutate a store's `data()` out from under it.
    return [...data].sort(
      (a, b) => order * compareFieldValues(resolveFieldData(a, field), resolveFieldData(b, field)),
    );
  });

  /** The rows actually rendered into `<tbody>`. */
  public readonly rowsToRender = computed<readonly T[]>(() => {
    const data = this.processed();
    const size = this.pageSize();
    if (this.lazy() || !this.paginator() || size <= 0) {
      return data;
    }
    const start = this.first();
    return data.slice(start, start + size);
  });

  /**
   * What the paginator reports on. A lazy table is told by the server; a client table counts what
   * survived the filter, so filtering to 3 rows while sitting on page 3 repaginates instead of
   * going blank.
   */
  public readonly totalRecords = computed(() =>
    this.lazy() ? (this.src.totalRecords() ?? this.src.value().length) : this.processed().length,
  );

  // User intent — the only things that ask for a new page of data.

  /**
   * Sorts by `field`. A fresh column starts ascending; the same column toggles. The page reset and
   * the sort happen in the same event, so a lazy table issues exactly one request.
   */
  public sort(field: string): void {
    const isSameColumn = this.sortField() === field;
    this.sortOrder.set(isSameColumn ? this.effectiveSortOrder() * -1 : 1);
    this.sortField.set(field);
    this.first.set(0);
    this.warnIfSortFieldUnresolvable(field);
    if (this.lazy()) {
      this.emit();
    }
  }

  public isSortedBy(field: string): boolean {
    return this.sortField() === field;
  }

  /** @param first 0-indexed row offset, not a page number. */
  public setPage(first: number): void {
    this.first.set(first);
    if (this.lazy()) {
      this.emit();
    }
  }

  /** Changing the page size returns to the first row, in the same event. */
  public setPageSize(size: number): void {
    this.size.set(size);
    this.first.set(0);
    if (this.lazy()) {
      this.emit();
    }
  }

  /** Multi-field, case-insensitive "contains". Resets to the first row — see {@link totalRecords}. */
  public filterGlobal(value: string): void {
    this.globalFilter.set(value);
    this.first.set(0);
    if (this.lazy()) {
      this.emit();
    }
  }

  /**
   * Selection is keyed by `dataKey`, never by object reference: a refetch hands back new instances,
   * and reference identity would silently drop every selected row.
   *
   * Without a `dataKey` we fall back to the row object itself. The absence of a `dataKey` must not
   * reach `resolveFieldData`, which would resolve `undefined` and collapse every row onto one key.
   */
  private keyOf(row: T): unknown {
    const dataKey = this.src.dataKey();
    return dataKey ? resolveFieldData(row, dataKey) : row;
  }

  private readonly selectedRows = computed<readonly T[]>(() => {
    const selection: unknown = this.src.selection();
    if (selection == null) {
      return [];
    }
    return (Array.isArray(selection) ? selection : [selection]) as readonly T[];
  });

  private readonly selectedKeys = computed(
    () => new Set(this.selectedRows().map((row) => this.keyOf(row))),
  );

  public isSelected(row: T): boolean {
    return this.selectedKeys().has(this.keyOf(row));
  }

  /** Whether anything at all is selected — drives `uiSelectableRow`'s roving tabindex. */
  public readonly hasSelection = computed(() => this.selectedRows().length > 0);

  /** Whether the table has no rows at all; the select-all checkbox is disabled when it does not. */
  public readonly isEmpty = computed(() => this.src.value().length === 0);

  /**
   * Toggles one row and preserves the rest, appending at the end. Individually-checked rows
   * therefore survive paging — unlike select-all, which replaces.
   */
  public toggleRow(row: T): void {
    if (this.selectionMode() === "single") {
      this.src.selection.set(this.isSelected(row) ? undefined : row);
      return;
    }
    const key = this.keyOf(row);
    const current = this.selectedRows();
    this.src.selection.set(
      this.selectedKeys().has(key)
        ? current.filter((selected) => this.keyOf(selected) !== key)
        : [...current, row],
    );
  }

  /** Single-select row click (`[uiSelectableRow]`). */
  public selectRow(row: T): void {
    const mode = this.selectionMode();
    if (!mode) {
      return;
    }
    if (mode === "single") {
      this.src.selection.set(this.isSelected(row) ? undefined : row);
      return;
    }
    this.toggleRow(row);
  }

  /**
   * Checked only when every processed row is selected. A partial selection renders it unchecked —
   * there is no indeterminate/tri-state, and adding one is a behaviour change, not a bug fix.
   * Pinned by the contract spec.
   */
  public readonly allSelected = computed(() => {
    const data = this.processed();
    return (
      data.length > 0 && this.selectedRows().length > 0 && data.every((row) => this.isSelected(row))
    );
  });

  /**
   * Replaces the selection with exactly the processed rows; it does not merge. Anything selected on
   * another page is discarded — bulk actions rely on getting exactly these rows back.
   */
  public toggleAll(checked: boolean): void {
    if (this.src.value().length === 0) {
      return;
    }
    this.src.selection.set(checked ? [...this.processed()] : []);
  }

  public isExpanded(row: T): boolean {
    return this.expandedKeys().has(this.keyOf(row));
  }

  public toggleExpanded(row: T): void {
    const key = this.keyOf(row);
    const next = new Set(this.expandedKeys());
    if (!next.delete(key)) {
      next.add(key);
    }
    this.expandedKeys.set(next);
  }

  /**
   * Dev-only. A sort path that resolves `undefined` for every row is a silent no-op sort — usually
   * a server-side column name ("CreatedDate") pasted into a client-side table.
   */
  private warnIfSortFieldUnresolvable(field: string): void {
    if (!isDevMode() || this.lazy() || this.warnedSortFields.has(field)) {
      return;
    }
    const data = this.src.value();
    if (data.length > 0 && data.every((row) => resolveFieldData(row, field) === undefined)) {
      this.warnedSortFields.add(field);
      console.warn(
        `[ui-data-table] Sorting by "${field}" resolved undefined for every row, so this sort ` +
          `does nothing. On a client-side table the sort field is a path into the ROW OBJECT ` +
          `("customer.name"), not a server column name.`,
      );
    }
  }
}
