import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getTruckById } from "@logistics/shared/api";
import type { DocumentType, TruckDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ToastModule } from "primeng/toast";
import { DocumentManager } from "@/shared/components";

@Component({
  selector: "app-truck-documents",
  templateUrl: "./truck-documents.html",
  imports: [CardModule, ToastModule, RouterLink, DocumentManager, ButtonModule],
})
export class TruckDocumentsPage implements OnInit {
  private readonly api = inject(Api);

  protected readonly truck = signal<TruckDto | null>(null);

  public readonly id = input.required<string>();

  // Note: New truck-specific types (dot_inspection, title_certificate, etc.) require
  // regenerating API types after backend deployment
  protected readonly truckDocTypes = [
    "vehicle_registration",
    "insurance_certificate",
    "dot_inspection",
    "title_certificate",
    "lease_agreement",
    "maintenance_record",
    "annual_inspection",
    "photo",
    "other",
  ] as DocumentType[];

  ngOnInit(): void {
    this.fetchTruck();
  }

  private async fetchTruck(): Promise<void> {
    const result = await this.api.invoke(getTruckById, { truckOrDriverId: this.id() });
    if (result) {
      this.truck.set(result);
    }
  }
}
