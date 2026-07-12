import { Component, computed, inject, input, output } from "@angular/core";
import { UiSelectField } from "../form/select-field/select-field";
import { Icon } from "../icons/icon/icon";
import { buttonVariants } from "../primitives/button";
import { HlmPagination, HlmPaginationContent, HlmPaginationItem } from "../primitives/pagination";
import { UiTableState } from "./table-state";

/**
 * The paginator for `<ui-data-table>`.
 *
 * Built on Helm's PRESENTATIONAL pagination directives (`hlmPagination` / `hlmPaginationContent` /
 * `hlmPaginationItem`) plus `buttonVariants`. The `hlm-numbered-pagination` and
 * `hlm-pagination-link` / `-previous` / `-next` / `-ellipsis` leaves were vendored but never used —
 * this component pages by STATE, not by URL, and those leaves render `<a>` elements via a
 * `RouterLink` host directive — so they were removed from the vendored primitive. This component
 * borrows their look (`buttonVariants`) without their routing.
 */
@Component({
  selector: "ui-table-paginator",
  templateUrl: "./table-paginator.html",
  imports: [HlmPagination, HlmPaginationContent, HlmPaginationItem, Icon, UiSelectField],
  host: { class: "flex items-center justify-between gap-4 flex-wrap py-2" },
})
export class UiTablePaginator {
  private readonly state = inject(UiTableState);

  /**
   * Page-size choices. Rendered ONLY when a consumer passes them — matching `<p-paginator>`'s
   * `*ngIf="rowsPerPageOptions"`. It is [10, 25, 50] at all 41 sites that bind it, but it is NOT
   * hardcoded: ~50 tables bind nothing and must keep showing NO page-size control. (It would also
   * break the contract spec, whose client host runs `rows = 2`: a page-size trigger is itself a
   * `<button>` reading "2", and the spec finds page buttons by their text.)
   */
  public readonly rowsPerPageOptions = input<number[] | undefined>(undefined);
  public readonly showCurrentPageReport = input(false);
  public readonly currentPageReportTemplate = input<string>("{currentPage} of {totalPages}");

  /** Raised when the user picks a page size, so the table can re-request data. */
  public readonly pageSizeChange = output<number>();

  protected readonly totalRecords = computed(() => this.state.totalRecords());
  protected readonly pageSize = computed(() => this.state.pageSize());

  protected readonly pageCount = computed(() => {
    const size = this.pageSize();
    return size > 0 ? Math.ceil(this.totalRecords() / size) : 0;
  });

  /** 0-indexed. `state.first` is a row OFFSET, not a page number. */
  protected readonly page = computed(() => {
    const size = this.pageSize();
    return size > 0 ? Math.floor(this.state.first() / size) : 0;
  });

  protected readonly isFirstPage = computed(() => this.page() === 0);
  protected readonly isLastPage = computed(() => this.page() >= this.pageCount() - 1);

  /**
   * The windowed page links, reproducing `<p-paginator>`'s `getPageLinkBoundaries()` with its
   * default `pageLinkSize` of 5.
   */
  protected readonly pageLinks = computed<number[]>(() => {
    const pageCount = this.pageCount();
    const visible = Math.min(5, pageCount);
    let start = Math.max(0, Math.ceil(this.page() - visible / 2));
    const end = Math.min(pageCount - 1, start + visible - 1);
    start = Math.max(0, start - (5 - (end - start + 1)));
    const links: number[] = [];
    for (let i = start; i <= end; i++) {
      links.push(i);
    }
    return links;
  });

  /**
   * The "1 to 10 of 57" report. 34 templates show it; 2 pass a custom string.
   *
   * ⚠ `{first}` and `{last}` are 1-INDEXED ROW NUMBERS, while `state.first` is a 0-INDEXED
   * OFFSET. Reusing the name across both units is a guaranteed off-by-one on 34 tables, so the
   * conversion happens here, once, and nowhere else.
   */
  protected readonly pageReport = computed(() => {
    const total = this.totalRecords();
    const offset = this.state.first();
    const size = this.pageSize();
    const firstRow = total === 0 ? 0 : offset + 1;
    const lastRow = size > 0 ? Math.min(offset + size, total) : total;
    return this.currentPageReportTemplate()
      .replace("{currentPage}", String(this.pageCount() === 0 ? 0 : this.page() + 1))
      .replace("{totalPages}", String(this.pageCount()))
      .replace("{first}", String(firstRow))
      .replace("{last}", String(lastRow))
      .replace("{totalRecords}", String(total));
  });

  protected readonly linkClass = (isActive: boolean): string =>
    buttonVariants({ variant: isActive ? "outline" : "ghost", size: "icon" });

  protected goToPage(page: number): void {
    this.state.setPage(page * this.pageSize());
  }

  protected onPageSizeChange(size: number | null): void {
    if (size != null) {
      this.state.setPageSize(size);
      this.pageSizeChange.emit(size);
    }
  }
}
