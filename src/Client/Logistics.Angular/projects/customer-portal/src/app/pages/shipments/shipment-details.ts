import { Component, effect, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import {
  Api,
  getPortalLoad,
  getPortalLoadDocuments,
  type DocumentDto,
  type PortalLoadDto,
} from "@logistics/shared/api";
import { Grid, Icon, Stack, Surface, Typography } from "@logistics/shared/components";
import { CurrencyFormatPipe, DateFormatPipe, DistanceUnitPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";

@Component({
  selector: "cp-shipment-details",
  templateUrl: "./shipment-details.html",
  imports: [
    CurrencyFormatPipe,
    DateFormatPipe,
    DistanceUnitPipe,
    RouterLink,
    CardModule,
    ButtonModule,
    TagModule,
    TableModule,
    ProgressSpinnerModule,
    Grid,
    Icon,
    Stack,
    Surface,
    Typography,
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

  protected getStatusSeverity(status: string | undefined): "success" | "info" | "warn" | "danger" {
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
