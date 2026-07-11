import { CommonModule } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInspection, type ConditionReportDto } from "@logistics/shared/api";
import {
  Badge,
  Card,
  Divider,
  Grid,
  Icon,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiLightbox,
  type UiBadgeIntent,
  type UiLightboxImage,
} from "@logistics/shared/ui";
import { isContainerLoadType } from "@logistics/shared/utils";
import { PageHeader } from "@/shared/components";
import { ConditionDefectsList } from "@/shared/components/inspections";

@Component({
  selector: "app-condition-report-detail",
  templateUrl: "./condition-report-detail.html",
  imports: [
    Badge,
    Card,
    CommonModule,
    ConditionDefectsList,
    Divider,
    Grid,
    Icon,
    PageHeader,
    RouterModule,
    Spinner,
    Stack,
    Typography,
    UiButton,
    UiLightbox,
  ],
})
export class ConditionReportDetailPage implements OnInit {
  private readonly api = inject(Api);

  public readonly id = input.required<string>();
  public readonly isLoading = signal(false);
  public readonly report = signal<ConditionReportDto | null>(null);
  public readonly showGallery = signal(false);
  public readonly activeImageIndex = signal(0);

  public readonly isVehicleLoad = computed(() => this.report()?.loadType === "vehicle");

  public readonly isContainerLoad = computed(() => isContainerLoadType(this.report()?.loadType));

  public readonly photoUrls = computed<UiLightboxImage[]>(() => {
    const r = this.report();
    if (!r?.photos) {
      return [];
    }

    return r.photos.map((p) => ({
      src: this.getPhotoUrl(p.blobPath),
      thumbnailSrc: this.getPhotoUrl(p.blobPath),
      alt: p.originalFileName || p.fileName || "Photo",
    }));
  });

  getPhotoUrl(blobPath?: string | null): string {
    if (!blobPath) return "";
    return `/api/documents/download?path=${encodeURIComponent(blobPath)}`;
  }

  ngOnInit(): void {
    this.fetchReport();
  }

  getTypeLabel(type: string): string {
    switch (type) {
      case "Pickup":
        return "Pickup Inspection";
      case "Delivery":
        return "Delivery Inspection";
      default:
        return type;
    }
  }

  getTypeSeverity(type: string): UiBadgeIntent {
    switch (type) {
      case "Pickup":
        return "info";
      case "Delivery":
        return "success";
      default:
        return "secondary";
    }
  }

  getCargoTypeLabel(): string {
    const t = this.report()?.loadType;
    if (!t) return "Cargo";
    switch (t) {
      case "vehicle":
        return "Vehicle";
      case "intermodal_container":
        return "Intermodal Container";
      case "tank_container":
        return "Tank Container";
      case "reefer_container":
        return "Reefer Container";
      default:
        return "Freight";
    }
  }

  getVehicleInfo(): string {
    const r = this.report();
    if (!r) return "N/A";
    const parts = [r.vehicleYear, r.vehicleMake, r.vehicleModel].filter(Boolean);
    return parts.length > 0 ? parts.join(" ") : "N/A";
  }

  getGoogleMapsUrl(lat?: number | null, lng?: number | null): string {
    if (!lat || !lng) return "";
    return `https://www.google.com/maps?q=${lat},${lng}`;
  }

  formatCoordinates(lat?: number | null, lng?: number | null): string {
    if (!lat || !lng) return "N/A";
    return `${lat.toFixed(6)}, ${lng.toFixed(6)}`;
  }

  openGallery(index: number): void {
    this.activeImageIndex.set(index);
    this.showGallery.set(true);
  }

  private async fetchReport(): Promise<void> {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    const result = await this.api.invoke(getInspection, { id: this.id() });
    if (result) {
      this.report.set(result);
    }
    this.isLoading.set(false);
  }
}
