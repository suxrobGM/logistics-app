import { CommonModule, CurrencyPipe } from "@angular/common";
import { Component, type OnInit, inject, input, signal } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getDocuments, getTruckById } from "@logistics/shared/api";
import type {
  DailyGrossesDto,
  DocumentDto,
  DocumentType,
  MonthlyGrossesDto,
  TruckDto,
} from "@logistics/shared/api";
import type { TruckGeolocationDto } from "@logistics/shared/api/models";
import { AddressPipe } from "@logistics/shared/pipes";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TabsModule } from "primeng/tabs";
import { TooltipModule } from "primeng/tooltip";
import {
  type BarChartDrawnEvent,
  DocumentManagerComponent,
  GeolocationMap,
  GrossBarchart,
  PageHeader,
  TruckStatusTag,
  TruckTypeTag,
} from "@/shared/components";
import { DistanceUnitPipe } from "@/shared/pipes";
import {
  DocumentStatusOverview,
  type LineChartDrawnEvent,
  TruckGrossLinechart,
  TruckLoadsList,
} from "../components";

@Component({
  selector: "app-truck-details",
  templateUrl: "./truck-details.html",
  styleUrl: "./truck-details.css",
  imports: [
    CommonModule,
    ButtonModule,
    CardModule,
    TooltipModule,
    TabsModule,
    DividerModule,
    ProgressSpinnerModule,
    RouterLink,
    CurrencyPipe,
    AddressPipe,
    DistanceUnitPipe,
    GeolocationMap,
    TruckGrossLinechart,
    GrossBarchart,
    PageHeader,
    TruckStatusTag,
    TruckTypeTag,
    DocumentManagerComponent,
    TruckLoadsList,
    DocumentStatusOverview,
  ],
})
export class TruckDetailsComponent implements OnInit {
  private readonly api = inject(Api);

  protected readonly id = input<string>();
  protected readonly isLoading = signal(false);
  protected readonly truck = signal<TruckDto | null>(null);
  protected readonly dailyGrosses = signal<DailyGrossesDto | null>(null);
  protected readonly monthlyGrosses = signal<MonthlyGrossesDto | null>(null);
  protected readonly rpmCurrent = signal(0);
  protected readonly rpmAllTime = signal(0);
  protected readonly truckLocations = signal<TruckGeolocationDto[]>([]);
  protected readonly documents = signal<DocumentDto[]>([]);
  protected readonly activeTab = signal(0);

  protected readonly truckDocTypes: DocumentType[] = [
    "vehicle_registration",
    "insurance_certificate",
    "dot_inspection",
    "title_certificate",
    "lease_agreement",
    "maintenance_record",
    "annual_inspection",
    "photo",
    "other",
  ];

  ngOnInit(): void {
    this.fetchTruck();
    this.fetchDocuments();
  }

  onTabChange(index: unknown): void {
    this.activeTab.set(index as number);
  }

  onLineChartDrawn(event: LineChartDrawnEvent): void {
    this.dailyGrosses.set(event.dailyGrosses);
    this.rpmCurrent.set(event.rpm);
  }

  onBarChartDrawn(event: BarChartDrawnEvent): void {
    this.monthlyGrosses.set(event.monthlyGrosses);
    this.rpmAllTime.set(event.rpm);
  }

  private async fetchTruck(): Promise<void> {
    const id = this.id();

    if (!id) {
      return;
    }

    this.isLoading.set(true);

    const truck = await this.api.invoke(getTruckById, { truckOrDriverId: id });
    if (truck) {
      this.truck.set(truck);

      this.truckLocations.set([
        {
          currentLocation: truck.currentLocation,
          truckId: truck.id,
          truckNumber: truck.number,
          driversName: [truck.mainDriver?.fullName, truck.secondaryDriver?.fullName]
            .filter(Boolean)
            .join(", "),
        },
      ]);
    }

    this.isLoading.set(false);
  }

  private async fetchDocuments(): Promise<void> {
    const id = this.id();
    if (!id) {
      return;
    }

    const docs = await this.api.invoke(getDocuments, {
      OwnerType: "truck",
      OwnerId: id,
    });

    if (docs) {
      this.documents.set(docs);
    }
  }
}
