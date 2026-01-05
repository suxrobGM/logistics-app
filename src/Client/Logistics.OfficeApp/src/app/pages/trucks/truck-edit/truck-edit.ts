import { Component, type OnInit, computed, inject, input, signal } from "@angular/core";
import {
  FormControl,
  FormGroup,
  FormsModule,
  ReactiveFormsModule,
  Validators,
} from "@angular/forms";
import { Router, RouterLink } from "@angular/router";
import { AutoCompleteModule } from "primeng/autocomplete";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { InputTextModule } from "primeng/inputtext";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { ToastModule } from "primeng/toast";
import {
  Api,
  getTruckById$Json,
  createTruck$Json,
  updateTruck$Json,
  deleteTruck$Json,
  getEmployees$Json,
} from "@/core/api";
import {
  type CreateTruckCommand,
  type EmployeeDto,
  type TruckStatus,
  type TruckType,
  type UpdateTruckCommand,
  truckStatusOptions,
  truckTypeOptions,
} from "@/core/api/models";
import { ToastService } from "@/core/services";
import { FormField } from "@/shared/components";

@Component({
  selector: "app-truck-edit",
  templateUrl: "./truck-edit.html",
  imports: [
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    RouterLink,
    FormField,
    InputTextModule,
    SelectModule,
  ],
})
export class TruckEditComponent implements OnInit {
  protected readonly truckTypes = truckTypeOptions;
  protected readonly truckStatuses = truckStatusOptions;

  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly id = input<string | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly suggestedDrivers = signal<EmployeeDto[]>([]);
  protected readonly title = computed(() => (this.id() ? "Edit a truck" : "Add a new truck"));

  protected readonly form = new FormGroup({
    truckNumber: new FormControl<string>("", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckType: new FormControl<TruckType>("freight_truck", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckStatus: new FormControl<TruckStatus>("available", {
      validators: [Validators.required],
      nonNullable: true,
    }),
    mainDriver: new FormControl<EmployeeDto | null>(null),
    secondaryDriver: new FormControl<EmployeeDto | null>(null),
  });

  ngOnInit(): void {
    const id = this.id();

    if (id) {
      this.fetchTruck(id);
    }
  }

  protected async searchDriver(event: { query: string }): Promise<void> {
    const result = await this.api.invoke(getEmployees$Json, { Search: event.query, Role: "Driver" });
    if (result.success && result.data) {
      this.suggestedDrivers.set(result.data);
    }
  }

  protected submit(): void {
    if (this.id()) {
      this.updateTruck();
    } else {
      this.createTruck();
    }
  }

  protected confirmToDelete(): void {
    this.toastService.confirm({
      message: "Are you sure that you want to delete this truck?",
      accept: () => this.deleteTruck(),
    });
  }

  private async fetchTruck(id: string): Promise<void> {
    const result = await this.api.invoke(getTruckById$Json, { truckOrDriverId: id });
    if (result.success && result.data) {
      const truck = result.data;

      this.form.patchValue({
        truckNumber: truck.number ?? undefined,
        truckStatus: truck.status,
        truckType: truck.type,
        mainDriver: truck.mainDriver,
        secondaryDriver: truck.secondaryDriver,
      });
    }
  }

  private async createTruck(): Promise<void> {
    this.isLoading.set(true);

    const command: CreateTruckCommand = {
      truckNumber: this.form.value.truckNumber!,
      truckType: this.form.value.truckType!,
      mainDriverId: this.form.value.mainDriver?.id ?? undefined,
    };

    const result = await this.api.invoke(createTruck$Json, { body: command });
    if (result.success) {
      this.toastService.showSuccess("A new truck has been created successfully");
      this.router.navigateByUrl("/trucks");
    }

    this.isLoading.set(false);
  }

  private async updateTruck(): Promise<void> {
    this.isLoading.set(true);

    const updateTruckCommand: UpdateTruckCommand = {
      id: this.id()!,
      truckNumber: this.form.value.truckNumber,
      truckType: this.form.value.truckType,
      truckStatus: this.form.value.truckStatus,
      mainDriverId: this.form.value.mainDriver?.id,
      secondaryDriverId: this.form.value.secondaryDriver?.id,
    };

    const result = await this.api.invoke(updateTruck$Json, {
      id: this.id()!,
      body: updateTruckCommand,
    });
    if (result.success) {
      this.toastService.showSuccess("Truck has been updated successfully");
    }

    this.isLoading.set(false);
  }

  private async deleteTruck(): Promise<void> {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    const result = await this.api.invoke(deleteTruck$Json, { id: this.id()! });
    if (result.success) {
      this.toastService.showSuccess("A truck has been deleted successfully");
      this.router.navigateByUrl("/trucks");
    }

    this.isLoading.set(false);
  }
}
