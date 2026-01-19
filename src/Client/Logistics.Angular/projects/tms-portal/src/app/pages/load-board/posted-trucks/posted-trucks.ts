import { DatePipe } from "@angular/common";
import { Component, type OnInit, inject, signal } from "@angular/core";
import { FormBuilder, ReactiveFormsModule, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import {
  Api,
  type LoadBoardConfigurationDto,
  type LoadBoardProviderType,
  type PostedTruckDto,
  type TruckDto,
  getLoadBoardProviders,
  getPostedTrucks,
  getTrucks,
  postTruckToLoadBoard,
  removePostedTruck,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DatePickerModule } from "primeng/datepicker";
import { DialogModule } from "primeng/dialog";
import { InputNumberModule } from "primeng/inputnumber";
import { InputTextModule } from "primeng/inputtext";
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
  selector: "app-posted-trucks",
  templateUrl: "./posted-trucks.html",
  imports: [
    ButtonModule,
    CardModule,
    DatePipe,
    DatePickerModule,
    DialogModule,
    InputNumberModule,
    InputTextModule,
    LabeledField,
    PageHeader,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    SelectModule,
    TableModule,
    TagModule,
    TooltipModule,
  ],
})
export class PostedTrucksComponent implements OnInit {
  private readonly router = inject(Router);
  private readonly api = inject(Api);
  private readonly fb = inject(FormBuilder);
  private readonly toastService = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly posting = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly postedTrucks = signal<PostedTruckDto[]>([]);
  protected readonly trucks = signal<TruckDto[]>([]);
  protected readonly providers = signal<LoadBoardConfigurationDto[]>([]);
  protected readonly showPostDialog = signal(false);

  protected readonly equipmentOptions: EquipmentOption[] = [
    { label: "Dry Van", value: "Dry Van" },
    { label: "Flatbed", value: "Flatbed" },
    { label: "Reefer", value: "Reefer" },
    { label: "Step Deck", value: "Step Deck" },
    { label: "Lowboy", value: "Lowboy" },
    { label: "Car Hauler", value: "Car Hauler" },
    { label: "Box Truck", value: "Box Truck" },
  ];

  protected readonly postForm = this.fb.group({
    truckId: ["", Validators.required],
    providerType: ["demo" as LoadBoardProviderType, Validators.required],
    availableAtCity: ["", Validators.required],
    availableAtState: ["", Validators.required],
    availableAtZipCode: [""],
    destinationCity: [""],
    destinationState: [""],
    destinationRadius: [null as number | null],
    availableFrom: [new Date(), Validators.required],
    availableTo: [null as Date | null],
    equipmentType: [""],
    maxWeight: [null as number | null],
    maxLength: [null as number | null],
  });

  ngOnInit(): void {
    this.loadData();
  }

  protected async loadData(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const [postedTrucksData, trucksData, providersData] = await Promise.all([
        this.api.invoke(getPostedTrucks, {}),
        this.api.invoke(getTrucks, {}),
        this.api.invoke(getLoadBoardProviders),
      ]);

      this.postedTrucks.set(postedTrucksData ?? []);
      this.trucks.set(trucksData?.items ?? []);
      this.providers.set(providersData ?? []);
    } catch (err) {
      this.error.set("Failed to load data");
      console.error("Error loading data:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected openPostDialog(): void {
    this.postForm.reset({
      truckId: "",
      providerType: "demo" as LoadBoardProviderType,
      availableAtCity: "",
      availableAtState: "",
      availableAtZipCode: "",
      destinationCity: "",
      destinationState: "",
      destinationRadius: null,
      availableFrom: new Date(),
      availableTo: null,
      equipmentType: "",
      maxWeight: null,
      maxLength: null,
    });
    this.showPostDialog.set(true);
  }

  protected async postTruck(): Promise<void> {
    if (this.postForm.invalid) return;

    this.posting.set(true);
    try {
      const formValue = this.postForm.value;
      await this.api.invoke(postTruckToLoadBoard, {
        body: {
          truckId: formValue.truckId!,
          providerType: formValue.providerType as LoadBoardProviderType,
          availableAtAddress: {
            line1: "N/A",
            city: formValue.availableAtCity!,
            state: formValue.availableAtState!,
            zipCode: formValue.availableAtZipCode || "00000",
            country: "US",
          },
          availableAtLocation: {
            latitude: 0, // Would normally geocode the address
            longitude: 0,
          },
          destinationPreference: formValue.destinationCity
            ? {
                line1: "N/A",
                city: formValue.destinationCity,
                state: formValue.destinationState ?? "",
                zipCode: "00000",
                country: "US",
              }
            : undefined,
          destinationRadius: formValue.destinationRadius || undefined,
          availableFrom: formValue.availableFrom!.toISOString(),
          availableTo: formValue.availableTo?.toISOString(),
          equipmentType: formValue.equipmentType || undefined,
          maxWeight: formValue.maxWeight || undefined,
          maxLength: formValue.maxLength || undefined,
        },
      });
      this.showPostDialog.set(false);
      this.toastService.showSuccess("Truck posted successfully");
      await this.loadData();
    } catch (err) {
      console.error("Error posting truck:", err);
      this.toastService.showError("Failed to post truck");
    } finally {
      this.posting.set(false);
    }
  }

  protected confirmRemovePost(truck: PostedTruckDto): void {
    this.toastService.confirm({
      message: `Are you sure you want to remove this truck post from ${truck.providerName}?`,
      header: "Remove Posted Truck",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.removePost(truck.id!),
    });
  }

  protected async removePost(postedTruckId: string): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(removePostedTruck, { postedTruckId });
      this.toastService.showSuccess("Truck post removed successfully");
      await this.loadData();
    } catch (err) {
      console.error("Error removing post:", err);
      this.toastService.showError("Failed to remove truck post");
    } finally {
      this.loading.set(false);
    }
  }

  protected goBack(): void {
    this.router.navigate(["/loadboard"]);
  }

  protected getProviderOptions(): { label: string; value: LoadBoardProviderType }[] {
    return this.providers()
      .filter((p) => p.isActive)
      .map((p) => ({
        label: this.getProviderLabel(p.providerType),
        value: p.providerType as LoadBoardProviderType,
      }));
  }

  protected getProviderLabel(type?: LoadBoardProviderType): string {
    switch (type) {
      case "dat":
        return "DAT";
      case "truckstop":
        return "Truckstop";
      case "one_two3_loadboard":
        return "123Loadboard";
      case "demo":
        return "Demo";
      default:
        return type ?? "Unknown";
    }
  }

  protected getStatusSeverity(status?: string): "success" | "warn" | "danger" | "secondary" {
    switch (status) {
      case "available":
        return "success";
      case "booked":
        return "warn";
      case "expired":
        return "danger";
      default:
        return "secondary";
    }
  }
}
