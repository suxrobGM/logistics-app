import { Component, inject, signal, type OnInit } from "@angular/core";
import {
  Api,
  bookLoadBoardListing,
  searchLoadBoard,
  type LoadBoardBookingRequest,
  type LoadBoardListingDto,
  type SearchLoadBoardCommand,
} from "@logistics/shared/api";
import { Stack } from "@logistics/shared/components";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastService } from "@/core/services";
import { DashboardCard, EmptyState, PageHeader } from "@/shared/components";
import { BookLoadDialog, LoadBoardSearchFilters, LoadBoardSearchResults } from "../_components";
import { LoadBoardStore } from "../store";

@Component({
  selector: "app-loadboard-search",
  templateUrl: "./loadboard-search.html",
  imports: [
    BookLoadDialog,
    DashboardCard,
    EmptyState,
    LoadBoardSearchFilters,
    LoadBoardSearchResults,
    PageHeader,
    ProgressSpinnerModule,
    Stack,
  ],
})
export class LoadBoardSearchComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);
  protected readonly store = inject(LoadBoardStore);

  protected readonly searching = signal(false);
  protected readonly booking = signal(false);
  protected readonly listings = signal<LoadBoardListingDto[]>([]);
  protected readonly searched = signal(false);
  protected readonly showBookDialog = signal(false);
  protected readonly selectedListing = signal<LoadBoardListingDto | null>(null);

  ngOnInit(): void {
    void this.store.loadAll();
  }

  protected async onSearch(body: SearchLoadBoardCommand): Promise<void> {
    this.searching.set(true);
    try {
      const data = await this.api.invoke(searchLoadBoard, { body });
      this.listings.set(data?.listings ?? []);
      this.searched.set(true);
      if ((data?.listings?.length ?? 0) === 0) {
        this.toast.showSuccess("No loads found matching your criteria", "Info");
      }
    } catch (err) {
      console.error("Error searching loads:", err);
      this.toast.showError("Failed to search loads");
    } finally {
      this.searching.set(false);
    }
  }

  protected openBookDialog(listing: LoadBoardListingDto): void {
    this.selectedListing.set(listing);
    this.showBookDialog.set(true);
  }

  protected async onBook(body: LoadBoardBookingRequest): Promise<void> {
    const listing = this.selectedListing();
    if (!listing?.externalListingId) {
      return;
    }

    this.booking.set(true);
    try {
      await this.api.invoke(bookLoadBoardListing, {
        listingId: listing.externalListingId,
        body,
      });
      this.showBookDialog.set(false);
      this.toast.showSuccess("Load booked successfully! A new load has been created in your TMS.");
      this.listings.update((cur) =>
        cur.filter((l) => l.externalListingId !== listing.externalListingId),
      );
    } catch (err) {
      console.error("Error booking load:", err);
      this.toast.showError("Failed to book load");
    } finally {
      this.booking.set(false);
    }
  }
}
