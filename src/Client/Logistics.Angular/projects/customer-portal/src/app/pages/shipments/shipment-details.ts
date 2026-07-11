import { Component, effect, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  getPortalLoad,
  getPortalLoadDocuments,
  type DocumentDto,
  type PortalLoadDto,
} from "@logistics/shared/api";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import {
  Badge,
  Grid,
  Icon,
  Spinner,
  Stack,
  Surface,
  Typography,
  UiButton,
  UiDataTable,
  type UiBadgeIntent,
} from "@logistics/shared/ui";

@Component({
  selector: "cp-shipment-details",
  templateUrl: "./shipment-details.html",
  imports: [
    Badge,
    CurrencyFormatPipe,
    DateFormatPipe,
    DistanceUnitPipe,
    Grid,
    Icon,
    RouterLink,
    Spinner,
    Stack,
    Surface,
    Typography,
    UiButton,
    UiDataTable,
  ],
})
export class ShipmentDetails {
  private readonly api = inject(Api);

  protected readonly id = input.required<string>();
  protected readonly load = signal<PortalLoadDto | null>(null);
  protected readonly documents = signal<DocumentDto[]>([]);
  protected readonly isLoading = signal(true);

  constructor() {
    effect(() => {
      const loadId = this.id();
      if (loadId) {
        this.loadData(loadId);
      }
    });
  }

  private async loadData(loadId: string): Promise<void> {
    this.isLoading.set(true);
    try {
      const [loadData, docs] = await Promise.all([
        this.api.invoke(getPortalLoad, { loadId }),
        this.api.invoke(getPortalLoadDocuments, { loadId }),
      ]);

      this.load.set(loadData);
      this.documents.set(docs ?? []);
    } catch (error) {
      console.error("Failed to load shipment details:", error);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected getStatusSeverity(status: string | undefined): UiBadgeIntent {
    switch (status) {
      case "Delivered":
        return "success";
      case "PickedUp":
        return "info";
      case "Dispatched":
        return "warn";
      default:
        return "info";
    }
  }

  protected formatAddress(
    address:
      | {
          line1?: string | null;
          city?: string | null;
          state?: string | null;
          zipCode?: string | null;
        }
      | undefined,
  ): string {
    if (!address) return "-";
    const parts = [address.line1, address.city, address.state, address.zipCode].filter(Boolean);
    return parts.length > 0 ? parts.join(", ") : "-";
  }
}
