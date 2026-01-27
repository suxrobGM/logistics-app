import { DatePipe } from "@angular/common";
import { Component, computed, input } from "@angular/core";
import { RouterLink } from "@angular/router";
import type { LoadDto } from "@logistics/shared/api";
import { AddressPipe } from "@logistics/shared/pipes";
import { BadgeModule } from "primeng/badge";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { SkeletonModule } from "primeng/skeleton";

interface ScheduleItem {
  loadId: string;
  loadNumber: number;
  type: "pickup" | "delivery";
  date: Date | null;
  originCity: string;
  destinationCity: string;
  truckNumber: string | null;
}

@Component({
  selector: "app-my-schedule",
  templateUrl: "./my-schedule.html",
  imports: [CardModule, DividerModule, SkeletonModule, BadgeModule, RouterLink, DatePipe],
  providers: [AddressPipe],
})
export class MyScheduleComponent {
  readonly loads = input<LoadDto[]>([]);
  readonly isLoading = input(false);

  private readonly addressPipe = new AddressPipe();

  protected readonly scheduleItems = computed<ScheduleItem[]>(() => {
    const items: ScheduleItem[] = [];

    for (const load of this.loads()) {
      if (!load.id || !load.number) continue;

      const originCity = this.extractCity(load.originAddress);
      const destinationCity = this.extractCity(load.destinationAddress);

      // Add pickup item if load is dispatched (waiting for pickup)
      if (load.status === "dispatched") {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          type: "pickup",
          date: load.dispatchedAt ? new Date(load.dispatchedAt) : null,
          originCity,
          destinationCity,
          truckNumber: load.assignedTruckNumber ?? null,
        });
      }

      // Add delivery item if load is picked up (in transit)
      if (load.status === "picked_up") {
        items.push({
          loadId: load.id,
          loadNumber: load.number,
          type: "delivery",
          date: load.pickedUpAt ? new Date(load.pickedUpAt) : null,
          originCity,
          destinationCity,
          truckNumber: load.assignedTruckNumber ?? null,
        });
      }
    }

    // Sort by date, most recent first
    return items
      .sort((a, b) => {
        if (!a.date) return 1;
        if (!b.date) return -1;
        return b.date.getTime() - a.date.getTime();
      })
      .slice(0, 5);
  });

  private extractCity(address: LoadDto["originAddress"]): string {
    if (!address) return "Unknown";
    return address.city || this.addressPipe.transform(address) || "Unknown";
  }
}
