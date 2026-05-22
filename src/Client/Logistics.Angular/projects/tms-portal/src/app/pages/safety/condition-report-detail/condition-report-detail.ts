import { CommonModule } from "@angular/common";
import { Component, computed, inject, input, signal, type OnInit } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getInspection, type ConditionReportDto } from "@logistics/shared/api";
import { Grid, Icon, Stack, Typography } from "@logistics/shared/components";
import { isContainerLoadType } from "@logistics/shared/utils";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { GalleriaModule } from "primeng/galleria";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TagModule } from "primeng/tag";
import { PageHeader } from "@/shared/components";
import { ConditionDefectsList } from "@/shared/components/inspections";

@Component({
  selector: "app-condition-report-detail",
  templateUrl: "./condition-report-detail.html",
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    RouterModule,
    DividerModule,
    TagModule,
    GalleriaModule,
    PageHeader,
    ConditionDefectsList,
    Grid,
    Icon,
    Stack,
    Typography,
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

  public readonly photoUrls = computed(() => {
    const r = this.report();
    if (!r?.photos) {
      return [];
    }

    return r.photos.map((p) => ({
      itemImageSrc: this.getPhotoUrl(p.blobPath),
      thumbnailImageSrc: this.getPhotoUrl(p.blobPath),
      alt: p.originalFileName || p.fileName || "Photo",
      title: p.originalFileName || p.fileName || "Photo",
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

  getTypeSeverity(type: string): "info" | "success" | "warn" | "danger" | "secondary" | "contrast" {
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
