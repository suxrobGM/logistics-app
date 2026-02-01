import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, createTruck, deleteTruck, getTruckById, updateTruck } from "@logistics/shared/api";
import type { CreateTruckCommand, TruckDto, UpdateTruckCommand } from "@logistics/shared/api";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { ToastModule } from "primeng/toast";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { TruckForm, type TruckFormData } from "../components";

@Component({
  selector: "app-truck-edit",
  templateUrl: "./truck-edit.html",
  imports: [ToastModule, ConfirmDialogModule, ProgressSpinnerModule, PageHeader, TruckForm],
})
export class TruckEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  public readonly id = input<string | null>(null);
  protected readonly truck = signal<TruckDto | null>(null);
  protected readonly isLoading = signal(false);

  protected readonly mode = computed(() => (this.id() ? "edit" : "create") as "create" | "edit");
  protected readonly pageTitle = computed(() =>
    this.id() ? `Edit Truck #${this.truck()?.number ?? ""}` : "Add New Truck",
  );

  ngOnInit(): void {
    const id = this.id();
    if (id) {
      this.fetchTruck(id);
    }
  }

  protected async onSave(data: TruckFormData): Promise<void> {
    if (this.id()) {
      await this.updateTruck(data);
    } else {
      await this.createTruck(data);
    }
  }

  protected async onRemove(): Promise<void> {
    await this.deleteTruck();
  }

  private async fetchTruck(id: string): Promise<void> {
    this.isLoading.set(true);
    try {
      const truck = await this.api.invoke(getTruckById, { truckOrDriverId: id });
      this.truck.set(truck);
    } finally {
      this.isLoading.set(false);
    }
  }

  private async createTruck(data: TruckFormData): Promise<void> {
    this.isLoading.set(true);
    try {
      const command: CreateTruckCommand = {
        truckNumber: data.truckNumber,
        truckType: data.truckType,
        mainDriverId: data.mainDriver?.id ?? undefined,
        vehicleCapacity: data.vehicleCapacity,
        make: data.make,
        model: data.model,
        year: data.year,
        vin: data.vin,
        licensePlate: data.licensePlate,
        licensePlateState: data.licensePlateState,
      };

      await this.api.invoke(createTruck, { body: command });
      this.toastService.showSuccess("Truck has been created successfully");
      this.router.navigateByUrl("/trucks");
    } finally {
      this.isLoading.set(false);
    }
  }

  private async updateTruck(data: TruckFormData): Promise<void> {
    this.isLoading.set(true);
    try {
      const command: UpdateTruckCommand = {
        id: this.id()!,
        truckNumber: data.truckNumber,
        truckType: data.truckType,
        truckStatus: data.truckStatus,
        mainDriverId: data.mainDriver?.id,
        secondaryDriverId: data.secondaryDriver?.id,
        vehicleCapacity: data.vehicleCapacity,
        make: data.make ?? undefined,
        model: data.model ?? undefined,
        year: data.year ?? undefined,
        vin: data.vin ?? undefined,
        licensePlate: data.licensePlate ?? undefined,
        licensePlateState: data.licensePlateState ?? undefined,
      };

      await this.api.invoke(updateTruck, {
        id: this.id()!,
        body: command,
      });
      this.toastService.showSuccess("Truck has been updated successfully");
    } finally {
      this.isLoading.set(false);
    }
  }

  private async deleteTruck(): Promise<void> {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    try {
      await this.api.invoke(deleteTruck, { id: this.id()! });
      this.toastService.showSuccess("Truck has been deleted successfully");
      this.router.navigateByUrl("/trucks");
    } finally {
      this.isLoading.set(false);
    }
  }
}
