import { booleanAttribute, Component, computed, input } from "@angular/core";

export type GridSpacing = "0" | "1" | "2" | "3" | "4" | "6" | "8";
export type GridDirection = "row" | "col";
export type GridSizeValue = number | "auto" | "grow";
export type GridBreakpoint = "xs" | "sm" | "md" | "lg" | "xl";
export type GridSize = GridSizeValue | Partial<Record<GridBreakpoint, GridSizeValue>>;
export type GridOffset = number | Partial<Record<GridBreakpoint, number>>;

const colSpan: Record<number, string> = {
  1: "col-span-1",
  2: "col-span-2",
  3: "col-span-3",
  4: "col-span-4",
  5: "col-span-5",
  6: "col-span-6",
  7: "col-span-7",
  8: "col-span-8",
  9: "col-span-9",
  10: "col-span-10",
  11: "col-span-11",
  12: "col-span-12",
};

const smColSpan: Record<number, string> = {
  1: "sm:col-span-1",
  2: "sm:col-span-2",
  3: "sm:col-span-3",
  4: "sm:col-span-4",
  5: "sm:col-span-5",
  6: "sm:col-span-6",
  7: "sm:col-span-7",
  8: "sm:col-span-8",
  9: "sm:col-span-9",
  10: "sm:col-span-10",
  11: "sm:col-span-11",
  12: "sm:col-span-12",
};

const mdColSpan: Record<number, string> = {
  1: "md:col-span-1",
  2: "md:col-span-2",
  3: "md:col-span-3",
  4: "md:col-span-4",
  5: "md:col-span-5",
  6: "md:col-span-6",
  7: "md:col-span-7",
  8: "md:col-span-8",
  9: "md:col-span-9",
  10: "md:col-span-10",
  11: "md:col-span-11",
  12: "md:col-span-12",
};

const lgColSpan: Record<number, string> = {
  1: "lg:col-span-1",
  2: "lg:col-span-2",
  3: "lg:col-span-3",
  4: "lg:col-span-4",
  5: "lg:col-span-5",
  6: "lg:col-span-6",
  7: "lg:col-span-7",
  8: "lg:col-span-8",
  9: "lg:col-span-9",
  10: "lg:col-span-10",
  11: "lg:col-span-11",
  12: "lg:col-span-12",
};

const xlColSpan: Record<number, string> = {
  1: "xl:col-span-1",
  2: "xl:col-span-2",
  3: "xl:col-span-3",
  4: "xl:col-span-4",
  5: "xl:col-span-5",
  6: "xl:col-span-6",
  7: "xl:col-span-7",
  8: "xl:col-span-8",
  9: "xl:col-span-9",
  10: "xl:col-span-10",
  11: "xl:col-span-11",
  12: "xl:col-span-12",
};

const colStart: Record<number, string> = {
  1: "col-start-1",
  2: "col-start-2",
  3: "col-start-3",
  4: "col-start-4",
  5: "col-start-5",
  6: "col-start-6",
  7: "col-start-7",
  8: "col-start-8",
  9: "col-start-9",
  10: "col-start-10",
  11: "col-start-11",
  12: "col-start-12",
};

const smColStart: Record<number, string> = {
  1: "sm:col-start-1",
  2: "sm:col-start-2",
  3: "sm:col-start-3",
  4: "sm:col-start-4",
  5: "sm:col-start-5",
  6: "sm:col-start-6",
  7: "sm:col-start-7",
  8: "sm:col-start-8",
  9: "sm:col-start-9",
  10: "sm:col-start-10",
  11: "sm:col-start-11",
  12: "sm:col-start-12",
};

const mdColStart: Record<number, string> = {
  1: "md:col-start-1",
  2: "md:col-start-2",
  3: "md:col-start-3",
  4: "md:col-start-4",
  5: "md:col-start-5",
  6: "md:col-start-6",
  7: "md:col-start-7",
  8: "md:col-start-8",
  9: "md:col-start-9",
  10: "md:col-start-10",
  11: "md:col-start-11",
  12: "md:col-start-12",
};

const lgColStart: Record<number, string> = {
  1: "lg:col-start-1",
  2: "lg:col-start-2",
  3: "lg:col-start-3",
  4: "lg:col-start-4",
  5: "lg:col-start-5",
  6: "lg:col-start-6",
  7: "lg:col-start-7",
  8: "lg:col-start-8",
  9: "lg:col-start-9",
  10: "lg:col-start-10",
  11: "lg:col-start-11",
  12: "lg:col-start-12",
};

const xlColStart: Record<number, string> = {
  1: "xl:col-start-1",
  2: "xl:col-start-2",
  3: "xl:col-start-3",
  4: "xl:col-start-4",
  5: "xl:col-start-5",
  6: "xl:col-start-6",
  7: "xl:col-start-7",
  8: "xl:col-start-8",
  9: "xl:col-start-9",
  10: "xl:col-start-10",
  11: "xl:col-start-11",
  12: "xl:col-start-12",
};

const spacingX: Record<GridSpacing, string> = {
  "0": "gap-x-0",
  "1": "gap-x-1",
  "2": "gap-x-2",
  "3": "gap-x-3",
  "4": "gap-x-4",
  "6": "gap-x-6",
  "8": "gap-x-8",
};

const spacingY: Record<GridSpacing, string> = {
  "0": "gap-y-0",
  "1": "gap-y-1",
  "2": "gap-y-2",
  "3": "gap-y-3",
  "4": "gap-y-4",
  "6": "gap-y-6",
  "8": "gap-y-8",
};

const spacingClasses: Record<GridSpacing, string> = {
  "0": "gap-0",
  "1": "gap-1",
  "2": "gap-2",
  "3": "gap-3",
  "4": "gap-4",
  "6": "gap-6",
  "8": "gap-8",
};

const bpSpan: Record<GridBreakpoint, Record<number, string>> = {
  xs: colSpan,
  sm: smColSpan,
  md: mdColSpan,
  lg: lgColSpan,
  xl: xlColSpan,
};

const bpStart: Record<GridBreakpoint, Record<number, string>> = {
  xs: colStart,
  sm: smColStart,
  md: mdColStart,
  lg: lgColStart,
  xl: xlColStart,
};

const bpAuto: Record<GridBreakpoint, string> = {
  xs: "col-auto",
  sm: "sm:col-auto",
  md: "md:col-auto",
  lg: "lg:col-auto",
  xl: "xl:col-auto",
};

const bpGrow: Record<GridBreakpoint, string> = {
  xs: "col-span-full",
  sm: "sm:col-span-full",
  md: "md:col-span-full",
  lg: "lg:col-span-full",
  xl: "xl:col-span-full",
};

function spanClassFor(bp: GridBreakpoint, value: GridSizeValue): string {
  if (value === "auto") return bpAuto[bp];
  if (value === "grow") return bpGrow[bp];
  const n = Math.max(1, Math.min(12, value));
  return bpSpan[bp][n];
}

function startClassFor(bp: GridBreakpoint, offset: number): string {
  const n = Math.max(1, Math.min(12, offset + 1));
  return bpStart[bp][n];
}

/**
 * MUI v9-style 12-column responsive grid. One component for both container and item
 * roles via the `container` flag. Items use `[size]` (number, 'auto', 'grow', or
 * a breakpoint object like `{ xs: 12, md: 6 }`) and optional `[offset]`.
 */
@Component({
  selector: "ui-grid",
  templateUrl: "./grid.html",
  host: { "[class]": "classes()" },
})
export class Grid {
  public readonly container = input<boolean, unknown>(false, { transform: booleanAttribute });
  public readonly spacing = input<GridSpacing | null>(null);
  public readonly rowSpacing = input<GridSpacing | null>(null);
  public readonly columnSpacing = input<GridSpacing | null>(null);
  public readonly direction = input<GridDirection>("row");
  public readonly wrap = input<"wrap" | "nowrap">("wrap");

  public readonly size = input<GridSize | null>(null);
  public readonly offset = input<GridOffset | null>(null);

  protected readonly classes = computed(() =>
    this.container() ? this.containerClasses() : this.itemClasses(),
  );

  private containerClasses(): string {
    const parts = ["grid", "grid-cols-12"];
    const row = this.rowSpacing();
    const col = this.columnSpacing();

    if (row !== null || col !== null) {
      if (row !== null) {
        parts.push(spacingY[row]);
      }
      if (col !== null) {
        parts.push(spacingX[col]);
      }
    } else {
      const sp = this.spacing();
      if (sp !== null) {
        parts.push(spacingClasses[sp]);
      }
    }

    if (this.wrap() === "nowrap") {
      parts.push("flex-nowrap");
    }
    return parts.filter(Boolean).join(" ");
  }

  private itemClasses(): string {
    const parts: string[] = [];
    const size = this.size();
    const offset = this.offset();

    if (size !== null) {
      if (typeof size === "number" || size === "auto" || size === "grow") {
        parts.push(spanClassFor("xs", size));
      } else {
        for (const bp of ["xs", "sm", "md", "lg", "xl"] as GridBreakpoint[]) {
          const v = size[bp];
          if (v != null) {
            parts.push(spanClassFor(bp, v));
          }
        }
      }
    } else {
      parts.push(spanClassFor("xs", 12));
    }

    if (offset !== null) {
      if (typeof offset === "number") {
        parts.push(startClassFor("xs", offset));
      } else {
        for (const bp of ["xs", "sm", "md", "lg", "xl"] as GridBreakpoint[]) {
          const v = offset[bp];
          if (v != null) {
            parts.push(startClassFor(bp, v));
          }
        }
      }
    }

    return parts.join(" ");
  }
}
