import { Component, computed, Directive, ElementRef, inject, input } from "@angular/core";
import { UiTableState } from "./table-state";

/** Marks a row for the arrow-key traversal below. */
const SELECTABLE_ROW_ATTR = "data-ui-selectable-row";

/**
 * Makes a `<tr>` selectable by click and by keyboard.
 *
 * Implements a ROVING TABINDEX: with nothing selected every
 * row is tabbable (0); once there is a selection only the anchor row is, and the rest go to -1, so
 * Tab lands back where the user left off instead of walking the whole table.
 *
 * Arrow/Home/End move focus by walking sibling rows in the DOM — not by index into the data —
 * which is what keeps it correct when a table renders extra `<tr>`s (an expanded detail row, an
 * empty-state row) between the selectable ones.
 *
 * Range selection (shift/ctrl+shift) is deliberately NOT ported: nothing in this repo uses it.
 */
@Directive({
  selector: "tr[uiSelectableRow]",
  host: {
    [SELECTABLE_ROW_ATTR]: "true",
    class: "cursor-pointer",
    "[attr.tabindex]": "tabIndex()",
    "[attr.aria-selected]": "selected()",
    "[class.bg-muted]": "selected()",
    "(click)": "select()",
    "(keydown)": "onKeydown($event)",
  },
})
export class UiSelectableRow<T = unknown> {
  private readonly state = inject(UiTableState) as UiTableState<T>;
  private readonly host = inject(ElementRef<HTMLTableRowElement>);

  public readonly row = input.required<T>({ alias: "uiSelectableRow" });

  protected readonly selected = computed(() => this.state.isSelected(this.row()));

  /**
   * Roving tabindex: 0 while nothing is selected, otherwise 0 only for the selected (anchor) row.
   */
  protected readonly tabIndex = computed(() => {
    if (!this.state.selectionMode()) {
      return null;
    }
    return this.state.hasSelection() ? (this.selected() ? 0 : -1) : 0;
  });

  protected select(): void {
    this.state.selectRow(this.row());
  }

  protected onKeydown(event: KeyboardEvent): void {
    switch (event.key) {
      case "ArrowDown":
        this.focusRow(this.siblingRow("next"));
        event.preventDefault();
        break;
      case "ArrowUp":
        this.focusRow(this.siblingRow("previous"));
        event.preventDefault();
        break;
      case "Home":
        this.focusRow(this.edgeRow("first"));
        event.preventDefault();
        break;
      case "End":
        this.focusRow(this.edgeRow("last"));
        event.preventDefault();
        break;
      case "Enter":
      case " ": {
        // A checkbox or a link inside the row owns its own Space/Enter.
        const target = event.target as HTMLElement;
        if (
          target !== this.host.nativeElement &&
          target.closest("input, button, a, select, textarea")
        ) {
          return;
        }
        this.select();
        event.preventDefault();
        break;
      }
    }
  }

  private siblingRow(direction: "next" | "previous"): HTMLElement | null {
    let sibling =
      direction === "next"
        ? (this.host.nativeElement as HTMLElement).nextElementSibling
        : (this.host.nativeElement as HTMLElement).previousElementSibling;
    while (sibling && !sibling.hasAttribute(SELECTABLE_ROW_ATTR)) {
      sibling = direction === "next" ? sibling.nextElementSibling : sibling.previousElementSibling;
    }
    return sibling as HTMLElement | null;
  }

  private edgeRow(edge: "first" | "last"): HTMLElement | null {
    const rows = (this.host.nativeElement as HTMLElement)
      .closest("tbody")
      ?.querySelectorAll<HTMLElement>(`[${SELECTABLE_ROW_ATTR}]`);
    if (!rows?.length) {
      return null;
    }
    return edge === "first" ? rows[0] : rows[rows.length - 1];
  }

  /** Hand the tabstop to the newly focused row. */
  private focusRow(next: HTMLElement | null): void {
    if (!next) {
      return;
    }
    (this.host.nativeElement as HTMLElement).tabIndex = -1;
    next.tabIndex = 0;
    next.focus();
  }
}

/**
 * Expands / collapses a row's detail row. Applied to the trigger (a `<ui-button>` in trips-list),
 * not to the `<tr>` — click bubbles to the host either way.
 */
@Directive({
  selector: "[uiRowToggler]",
  host: {
    "[attr.aria-expanded]": "expanded()",
    "(click)": "toggle($event)",
  },
})
export class UiRowToggler<T = unknown> {
  private readonly state = inject(UiTableState) as UiTableState<T>;

  public readonly row = input.required<T>({ alias: "uiRowToggler" });

  protected readonly expanded = computed(() => this.state.isExpanded(this.row()));

  protected toggle(event: Event): void {
    // The row underneath is usually clickable too (trips-list navigates on row click).
    event.stopPropagation();
    this.state.toggleExpanded(this.row());
  }
}

/**
 * A row's selection checkbox. Toggles exactly ONE row and leaves the rest of the selection alone,
 * so individually-checked rows survive paging.
 */
@Component({
  selector: "ui-table-checkbox",
  template: `
    <input
      type="checkbox"
      class="border-input text-primary focus-visible:ring-ring size-4 cursor-pointer rounded-sm border align-middle focus-visible:ring-2 focus-visible:outline-none"
      [checked]="checked()"
      [attr.aria-label]="checked() ? 'Unselect row' : 'Select row'"
      (change)="toggle($event)"
      (click)="$event.stopPropagation()"
    />
  `,
  host: { class: "inline-flex items-center" },
})
export class UiTableCheckbox<T = unknown> {
  private readonly state = inject(UiTableState) as UiTableState<T>;

  public readonly value = input.required<T>();
  /** Accepted for call-site compatibility; the engine keys selection by `dataKey`, not by index. */
  public readonly index = input<number | undefined>(undefined);

  protected readonly checked = computed(() => this.state.isSelected(this.value()));

  protected toggle(event: Event): void {
    event.stopPropagation();
    this.state.toggleRow(this.value());
  }
}

/**
 * The select-all checkbox.
 *
 * ⚠ It REPLACES the selection with the table's rows — it does not merge, so anything selected on
 * another page is discarded. And it is strictly BINARY: a partial selection renders it UNCHECKED,
 * with no indeterminate state. Both are counter-intuitive, both are what ships, and both are
 * pinned by the contract spec. Do not "improve" either into a tri-state.
 */
@Component({
  selector: "ui-table-header-checkbox",
  template: `
    <input
      type="checkbox"
      class="border-input text-primary focus-visible:ring-ring size-4 cursor-pointer rounded-sm border align-middle focus-visible:ring-2 focus-visible:outline-none"
      [checked]="checked()"
      [disabled]="disabled()"
      [attr.aria-label]="checked() ? 'Unselect all rows' : 'Select all rows'"
      (change)="toggleAll($event)"
      (click)="$event.stopPropagation()"
    />
  `,
  host: { class: "inline-flex items-center" },
})
export class UiTableHeaderCheckbox {
  private readonly state = inject(UiTableState);

  protected readonly checked = computed(() => this.state.allSelected());
  protected readonly disabled = computed(() => this.state.isEmpty());

  protected toggleAll(event: Event): void {
    event.stopPropagation();
    this.state.toggleAll((event.target as HTMLInputElement).checked);
  }
}

/**
 * Row-level behaviours for `<ui-data-table>`. A const array, not an NgModule, so it drops straight
 * into a standalone component's `imports` — which is how every call site already lists it.
 */
export const UiTableRowDirectives = [
  UiSelectableRow,
  UiRowToggler,
  UiTableCheckbox,
  UiTableHeaderCheckbox,
] as const;
