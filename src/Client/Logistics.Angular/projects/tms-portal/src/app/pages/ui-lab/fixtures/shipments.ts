/**
 * The `/ui-lab` table fixture. Local data only — the lab never talks to the API.
 *
 * The shape is chosen to reproduce, on screen, the table bugs that a build and a unit-test run
 * both stay green through:
 *
 * - `customer.name` is a **dotted path**, so `<th uiSortHeader="customer.name">` proves the field
 *   resolver walks nested objects (a re-implementation that does `row[field]` renders blanks).
 * - `driver` holds both `null` and `undefined`, so a sorted column has holes — a comparator that
 *   dereferences them throws, and one that stringifies them sorts `"null"` in among the names.
 * - `code` holds numeric-looking **strings**, including "9", "10" and "100". A naive `a < b` (or
 *   `localeCompare`) comparator orders those "10", "100", "9" — which is what S11 must not do.
 */
export interface LabShipment {
  readonly id: string;
  /** Numeric-looking string. Sorting this column is the numeric-vs-lexicographic canary. */
  readonly code: string;
  /** Nested object, sorted through the dotted path `customer.name`. */
  readonly customer: { readonly name: string };
  /** Deliberately holey: some rows are `null`, some are `undefined`. */
  readonly driver: string | null | undefined;
  readonly status: string;
  readonly weightKg: number;
  readonly pickup: Date;
}

/** 1…51 — gives us the adjacent "9" / "10" pair that lexicographic sorting inverts. */
const SEQUENTIAL_REFS: readonly number[] = Array.from({ length: 51 }, (_, i) => i + 1);

/** Wider magnitudes so "100" and "1000" also land between the single digits under a bad sort. */
const WIDE_REFS: readonly number[] = [99, 100, 101, 110, 999, 1000];

/** Exactly 57 rows: enough to page (10/25/50) and to make a mis-sort visible on page 1. */
const REF_NUMBERS: readonly number[] = [...SEQUENTIAL_REFS, ...WIDE_REFS];

const CUSTOMER_NAMES: readonly string[] = [
  "Zenith Freight",
  "Acme Logistics",
  "Meridian Cargo",
  "Blue Ridge Haulers",
  "Yukon Transport",
  "Cascade Shipping",
  "Northwind Traders",
  "Delta Intermodal",
];

const DRIVER_NAMES: readonly string[] = [
  "Ada Lovelace",
  "Grace Hopper",
  "Alan Turing",
  "Katherine Johnson",
  "Barbara Liskov",
];

const STATUSES: readonly string[] = [
  "Draft",
  "Dispatched",
  "PickedUp",
  "InTransit",
  "Delivered",
  "Cancelled",
  "Exception",
];

/**
 * Every 7th row has an explicit `null` driver and every 11th (offset 4) has `undefined`, so both
 * flavours of "missing" appear in the same sortable column.
 */
function driverAt(index: number): string | null | undefined {
  if (index % 7 === 0) {
    return null;
  }
  if (index % 11 === 4) {
    return undefined;
  }
  return DRIVER_NAMES[index % DRIVER_NAMES.length];
}

export const LAB_SHIPMENTS: readonly LabShipment[] = REF_NUMBERS.map((ref, index) => ({
  id: `SHP-${String(index + 1).padStart(3, "0")}`,
  code: String(ref),
  customer: { name: CUSTOMER_NAMES[index % CUSTOMER_NAMES.length] },
  driver: driverAt(index),
  status: STATUSES[index % STATUSES.length],
  weightKg: 400 + ((index * 137) % 17_600),
  pickup: new Date(Date.UTC(2026, 0, 5 + index)),
}));

/** A short slice for the selection table — small enough that "select all" is verifiable by eye. */
export const LAB_SELECTABLE_SHIPMENTS: readonly LabShipment[] = LAB_SHIPMENTS.slice(0, 8);
