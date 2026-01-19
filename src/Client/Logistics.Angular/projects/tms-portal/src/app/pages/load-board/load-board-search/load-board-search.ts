import { DatePipe, DecimalPipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule } from "@angular/forms";
import { Router } from "@angular/router";
import {
  Api,
  type LoadBoardListingDto,
  type LoadBoardProviderType,
  type TruckDto,
  bookLoadBoardListing,
  getTrucks,
  searchLoadBoard,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
import { MultiSelectModule } from "primeng/multiselect";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { LabeledField, PageHeader } from "@/shared/components";

interface EquipmentOption {
  label: string;
  value: string;
}

@Component({
  selector: "app-load-board-search",
  templateUrl: "./load-board-search.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePipe,
    DatePickerModule,
    DecimalPipe,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    LabeledField,
    MultiSelectModule,
    PageHeader,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class LoadBoardSearchComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  protected readonly loading = signal(false);
  protected readonly searching = signal(false);
  protected readonly booking = signal(false);
  protected readonly listings = signal<LoadBoardListingDto[]>([]);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly showBookDialog = signal(false);
  protected readonly selectedListing = signal<LoadBoardListingDto | null>(null);

  protected readonly equipmentOptions: EquipmentOption[] = [
    { label: "Dry Van", value: "Dry Van" },
    { label: "Flatbed", value: "Flatbed" },
    { label: "Reefer", value: "Reefer" },
    { label: "Step Deck", value: "Step Deck" },
    { label: "Lowboy", value: "Lowboy" },
    { label: "Car Hauler", value: "Car Hauler" },
    { label: "Box Truck", value: "Box Truck" },
  ];

  protected readonly searchForm = this.fb.group({
    originCity: [""],
    originState: [""],
    originRadius: [50],
    destinationCity: [""],
    destinationState: [""],
    destinationRadius: [50],
    pickupDateStart: [null as Date | null],
    pickupDateEnd: [null as Date | null],
    equipmentTypes: [[] as string[]],
    maxResults: [50],
  });

  protected readonly bookForm = this.fb.group({
    truckId: [""],
    contactName: [""],
    contactPhone: [""],
    contactEmail: [""],
    notes: [""],
  });

  ngOnInit(): void {
    this.loadTrucks();
  }

  protected async loadTrucks(): Promise<void> {
    try {
      const data = await this.api.invoke(getTrucks, {});
      this.trucks.set(data?.items ?? []);
    } catch (err) {
      console.error("Error loading trucks:", err);
    }
  }

  protected async searchLoads(): Promise<void> {
    this.searching.set(true);
    try {
      const formValue = this.searchForm.value;
      const data = await this.api.invoke(searchLoadBoard, {
        body: {
          originCity: formValue.originCity || undefined,
          originState: formValue.originState || undefined,
          originRadius: formValue.originRadius || undefined,
          destinationCity: formValue.destinationCity || undefined,
          destinationState: formValue.destinationState || undefined,
          destinationRadius: formValue.destinationRadius || undefined,
          pickupDateStart: formValue.pickupDateStart?.toISOString(),
          pickupDateEnd: formValue.pickupDateEnd?.toISOString(),
          equipmentTypes: formValue.equipmentTypes?.length ? formValue.equipmentTypes : undefined,
          maxResults: formValue.maxResults || 50,
        },
      });
      this.listings.set(data?.listings ?? []);

      if (data?.listings?.length === 0) {
        this.toastService.showSuccess("No loads found matching your criteria", "Info");
      }
    } catch (err) {
      console.error("Error searching loads:", err);
      this.toastService.showError("Failed to search loads");
    } finally {
      this.searching.set(false);
    }
  }

  protected openBookDialog(listing: LoadBoardListingDto): void {
    this.selectedListing.set(listing);
    this.bookForm.reset({
      truckId: "",
      contactName: "",
      contactPhone: "",
      contactEmail: "",
      notes: "",
    });
    this.showBookDialog.set(true);
  }

  protected async bookLoad(): Promise<void> {
    const listing = this.selectedListing();
    if (!listing?.externalListingId) return;

    this.booking.set(true);
    try {
      const formValue = this.bookForm.value;
      await this.api.invoke(bookLoadBoardListing, {
        listingId: listing.externalListingId,
        body: {
          truckId: formValue.truckId || undefined,
          notes: formValue.notes || undefined,
        },
      });
      this.showBookDialog.set(false);
      this.toastService.showSuccess(
        "Load booked successfully! A new load has been created in your TMS.",
      );
      // Remove the booked listing from results
      this.listings.update((current) =>
        current.filter((l) => l.externalListingId !== listing.externalListingId),
      );
    } catch (err) {
      console.error("Error booking load:", err);
      this.toastService.showError("Failed to book load");
    } finally {
      this.booking.set(false);
    }
  }

  protected goBack(): void {
    this.router.navigate(["/loadboard"]);
  }

  protected formatCurrency(value?: number | null): string {
    if (value == null) return "-";
    return new Intl.NumberFormat("en-US", {
      style: "currency",
      currency: "USD",
    }).format(value);
  }

  protected formatDistance(listing: LoadBoardListingDto): string {
    if (!listing.originAddress || !listing.destinationAddress) return "-";
    const origin = `${listing.originAddress.city}, ${listing.originAddress.state}`;
    const dest = `${listing.destinationAddress.city}, ${listing.destinationAddress.state}`;
    return `${origin} → ${dest}`;
  }

  protected getProviderSeverity(
    type?: LoadBoardProviderType,
  ): "info" | "success" | "warn" | "secondary" {
    switch (type) {
      case "dat":
        return "info";
      case "truckstop":
        return "success";
      case "one_two3_loadboard":
        return "warn";
      default:
        return "secondary";
    }
  }
}
