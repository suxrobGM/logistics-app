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
 * Passed in rather than injected: `UiDataTable` provides `UiTableState` on its own node, so a
 * `inject(UiDataTable)` inside the state would be a construction cycle (NG0200) the moment the
 * component itself injects the state for its template.
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

/**
 * Resolves a possibly-dotted field path ("customer.name", "originAddress.line1") against a row.
 *
 * Reproduces PrimeNG's `ObjectUtils.resolveFieldData`. Six client-sorted templates and the eight
 * sort headers of trips-list's nested table pass dotted paths.
 */
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
 * PrimeNG's `sortSingle` comparator, reproduced EXACTLY. Every clause here is characterization:
 *
 *  • **Nullish sorts FIRST on ascending** (`null` vs non-null yields -1, then the whole result is
 *    multiplied by the order). Descending therefore puts the holes last. This is what ships.
 *  • **Strings compare with `localeCompare`, which is NOT numeric-aware** — so a column of
 *    numeric-ish strings ("Load #", "Truck #") orders "10", "100", "9". A WART, pinned by the
 *    contract spec and made visible by the /ui-lab fixture. Do not "fix" it here: six templates
 *    depend on today's ordering, and making it numeric-aware is a separate, visible decision.
 *  • Real numbers fall through to `<` / `>`, so they sort numerically.
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

/** PrimeNG's "contains" match mode: accent-insensitive, case-insensitive substring. */
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
 * The table ENGINE: everything `<p-table>` used to do silently.
 *
 * Owns seven things Helm's presentational table directives do not provide — client sort, client
 * paging, a client global filter, selection, row expansion, the sort/page state a `<th
 * uiSortHeader>` reads, and the single lazy-load request the 33 server-paged pages live on.
 *
 * Provided on `<ui-data-table>`'s own node, which is what lets a `<th uiSortHeader>` written in a
 * CONSUMER's `<ng-template #header>` inject it: the template is DECLARED as a child of the
 * `<ui-data-table>` element, and `createEmbeddedView` parents the embedded view's injector at the
 * declaration node. For trips-list's nested table the inner element is a strictly closer ancestor
 * of its own templates, so the inner state shadows the outer one structurally. (Both properties
 * are pinned by `di-shadowing.probe.spec.ts`.)
 *
 * NOTHING here emits. The single `lazyLoad` output belongs to `UiDataTable`; this class calls the
 * `emit` callback handed to {@link connect}, and only ever from a user-intent method.
 */
@Injectable()
export class UiTableState<T = unknown> {
  private src!: UiTableSources<T>;
  private emit: () => void = () => undefined;

  // ── State ───────────────────────────────────────────────────────────────────────────────────
  // `linkedSignal`s SEEDED from the inputs. Seeding never emits, so a consumer's `[first]` or
  // `sortField="…"` is already in state by the time `ngOnInit` fires the one initial lazy load —
  // and a later input change (a store writing back `first`) re-seeds without emitting either.
  // They are lazy, so reading them before `connect()` is impossible in practice.

  /** 0-indexed row OFFSET of the current page. Not a page number. */
  public readonly first = linkedSignal(() => this.src.first());
  /** Page size. `undefined` when the consumer binds no `[rows]`, exactly as `<p-table>` leaves it. */
  public readonly size = linkedSignal(() => this.src.rows());
  public readonly sortField = linkedSignal(() => this.src.sortField());
  /**
   * `undefined` until something sorts — NOT 1.
   *
   * The initial lazy request must carry `sortOrder: undefined`; `createListStore` then applies its
   * own `?? -1`. Emitting PrimeNG's nominal default of `1` would silently change what the store
   * records having asked for. Pinned by the contract spec.
   */
  public readonly sortOrder = linkedSignal(() => this.src.sortOrder());
  private readonly globalFilter = signal("");
  private readonly expandedKeys = signal<ReadonlySet<unknown>>(new Set());
  private readonly warnedSortFields = new Set<string>();

  public connect(sources: UiTableSources<T>, emit: () => void): void {
    this.src = sources;
    this.emit = emit;
  }

  // ── Derived config ──────────────────────────────────────────────────────────────────────────

  public readonly lazy = computed(() => this.src.lazy());
  public readonly paginator = computed(() => this.src.paginator());
  public readonly selectionMode = computed(() => this.src.selectionMode());
  /**
   * 0 when unbound; every slice/page computation guards on it.
   *
   * ⚠ Reads {@link size}, the linkedSignal — NOT the raw `rows` input. `setPageSize()` writes
   * `size`, so a `pageSize` that read the input instead would bypass it: a LAZY table would still
   * limp along (its store writes the new size back through `[rows]`), but the 14 CLIENT-paged
   * tables have nothing to write back, so picking "25" would change the trigger, reset to page 1,
   * and still render 10 rows forever.
   */
  public readonly pageSize = computed(() => this.size() ?? 0);

  /**
   * The effective sort order. `sortOrder` stays `undefined` on the wire, but a client sort needs a
   * direction, and PrimeNG's own `defaultSortOrder` is 1.
   *
   * This also un-breaks a live wart. `<p-table>` guards its client sort with `if (field && order)`,
   * and the wrapper clobbers its `_sortOrder = 1` with an unbound `undefined` — so the two
   * templates that bind `sortField="order"` (trip-wizard-review, trip-details) have a sort header
   * that has NEVER sorted: the first click computes `undefined * -1` = `NaN`, which is falsy, so
   * the guard skips. Here a bound-but-orderless sort field simply means ascending, and the header
   * toggles from there. The stops those tables show are already stored in `order`, so ascending is
   * the order they already render in.
   */
  private readonly effectiveSortOrder = computed(() => this.sortOrder() ?? 1);

  // ── The client pipeline: filter -> sort -> slice ─────────────────────────────────────────────
  // For a LAZY table `value` IS the server's page: no filtering, no sorting, no slicing.

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
   * Filtered + sorted, but NOT paged — PrimeNG calls this `processedData`, and it is what
   * select-all and the header checkbox operate on (so select-all on a client-paged table takes
   * every matching row, not just the visible page).
   */
  public readonly processed = computed<readonly T[]>(() => {
    const data = this.filtered();
    const field = this.sortField();
    if (this.lazy() || !field) {
      return data;
    }
    const order = this.effectiveSortOrder();
    // Copy: `<p-table>` sorts the consumer's array IN PLACE. Renders identically, and not
    // mutating a store's `data()` out from under it is strictly safer.
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
   * survived the filter — filter to 3 rows while sitting on page 3 and the table must repaginate,
   * not go blank.
   */
  public readonly totalRecords = computed(() =>
    this.lazy() ? (this.src.totalRecords() ?? this.src.value().length) : this.processed().length,
  );

  // ── User intent. The ONLY things that ask for a new page of data. ────────────────────────────

  /**
   * Sorts by `field`. A FRESH column starts ASCENDING; the same column toggles. Sorting resets the
   * page offset to 0 in the SAME event, so a lazy table issues exactly ONE request.
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

  /** @param first 0-indexed row offset, NOT a page number. */
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

  // ── Selection ───────────────────────────────────────────────────────────────────────────────

  /**
   * Selection is keyed by `dataKey`, NEVER by object reference — a refetch hands back new object
   * instances, and reference identity would silently drop every selected row.
   *
   * When there is no `dataKey` we fall back to the row OBJECT as the key (reference identity).
   * Keying every row by a resolved `undefined` would collapse them all onto one key, so the
   * absence of a `dataKey` must not reach `resolveFieldData` at all.
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
   * Toggles ONE row and preserves the rest, appending at the end. Individually-checked rows
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
   * The select-all checkbox is checked only when EVERY processed row is selected. A PARTIAL
   * selection renders it UNCHECKED — there is no indeterminate/tri-state, and adding one would be
   * a behaviour change, not a bug fix. Pinned by the contract spec.
   */
  public readonly allSelected = computed(() => {
    const data = this.processed();
    return (
      data.length > 0 && this.selectedRows().length > 0 && data.every((row) => this.isSelected(row))
    );
  });

  /**
   * REPLACES the selection with exactly the processed rows — it does NOT merge. Anything selected
   * elsewhere (another page) is DISCARDED, because `selectionPageOnly` is set nowhere in this repo.
   * Bulk actions rely on getting exactly these rows back.
   */
  public toggleAll(checked: boolean): void {
    if (this.src.value().length === 0) {
      return;
    }
    this.src.selection.set(checked ? [...this.processed()] : []);
  }

  // ── Row expansion ───────────────────────────────────────────────────────────────────────────

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
   * Dev-only. A sort path that resolves `undefined` for EVERY row is a silent no-op sort — the
   * signature of a server-side column name ("CreatedDate") pasted into a client-side table, which
   * otherwise just quietly does nothing when clicked.
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
