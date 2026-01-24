import { CommonModule } from "@angular/common";
import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { RouterModule } from "@angular/router";
import { Api, getConditionReportById } from "@logistics/shared/api";
import type { ConditionReportDto, DamageMarkerDto } from "@logistics/shared/api/models";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { GalleriaModule } from "primeng/galleria";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import {
  type DamageMarker,
  VehicleDiagramComponent,
} from "@/shared/components/inspections/vehicle-diagram/vehicle-diagram";

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
    TableModule,
    TagModule,
    GalleriaModule,
    VehicleDiagramComponent,
  ],
})
export class ConditionReportDetailPage implements OnInit {
  private readonly api = inject(Api);

  readonly id = input.required<string>();
  readonly isLoading = signal(false);
  readonly report = signal<ConditionReportDto | null>(null);
  readonly showGallery = signal(false);
  readonly activeImageIndex = signal(0);

  readonly damageMarkers = computed<DamageMarker[]>(() => {
    const r = this.report();
    if (!r?.damageMarkers) return [];
    return r.damageMarkers
      .filter((m: DamageMarkerDto) => m.x !== undefined && m.y !== undefined)
      .map((m: DamageMarkerDto) => ({
        x: m.x!,
        y: m.y!,
        description: m.description ?? undefined,
        severity: m.severity ?? undefined,
      }));
  });

  readonly photoUrls = computed(() => {
    const r = this.report();
    if (!r?.photos) return [];
    return r.photos.map((p) => ({
      itemImageSrc: this.getPhotoUrl(p.blobPath),
      thumbnailImageSrc: this.getPhotoUrl(p.blobPath),
      alt: p.originalFileName || p.fileName || "Photo",
      title: p.originalFileName || p.fileName || "Photo",
    }));
  });

  getPhotoUrl(blobPath?: string | null): string {
    if (!blobPath) return "";
    // Construct URL from blob path - this assumes Azure Blob Storage pattern
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

  getSeverityClass(severity?: string): string {
    switch (severity?.toLowerCase()) {
      case "severe":
        return "text-red-600 dark:text-red-400 bg-red-500/10";
      case "moderate":
        return "text-orange-600 dark:text-orange-400 bg-orange-500/10";
      case "minor":
      default:
        return "text-yellow-600 dark:text-yellow-400 bg-yellow-500/10";
    }
  }

  getSeverityBadge(severity?: string): "danger" | "warn" | "info" {
    switch (severity?.toLowerCase()) {
      case "severe":
        return "danger";
      case "moderate":
        return "warn";
      case "minor":
      default:
        return "info";
    }
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
    const result = await this.api.invoke(getConditionReportById, { id: this.id() });
    if (result) {
      this.report.set(result);
    }
    this.isLoading.set(false);
  }
}
