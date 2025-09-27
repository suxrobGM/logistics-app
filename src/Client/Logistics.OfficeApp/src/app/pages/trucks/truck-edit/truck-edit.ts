import { Component, OnInit, computed, inject, input, signal } from "@angular/core";
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
import { ApiService } from "@/core/api";
import {
  CreateTruckCommand,
  EmployeeDto,
  TruckStatus,
  TruckType,
  UpdateTruckCommand,
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

  private readonly apiService = inject(ApiService);
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
    truckType: new FormControl<TruckType>(TruckType.FreightTruck, {
      validators: [Validators.required],
      nonNullable: true,
    }),
    truckStatus: new FormControl<TruckStatus>(TruckStatus.Available, {
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

  protected searchDriver(event: { query: string }): void {
    this.apiService.employeeApi.getDrivers({ search: event.query }).subscribe((result) => {
      if (result.success && result.data) {
        this.suggestedDrivers.set(result.data);
      }
    });
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

  private fetchTruck(id: string): void {
    this.apiService.truckApi.getTruck(id).subscribe((result) => {
      if (result.success && result.data) {
        const truck = result.data;

        this.form.patchValue({
          truckNumber: truck.number,
          truckStatus: truck.status,
          truckType: truck.type,
          mainDriver: truck.mainDriver,
          secondaryDriver: truck.secondaryDriver,
        });
      }
    });
  }

  private createTruck(): void {
    this.isLoading.set(true);

    const command: CreateTruckCommand = {
      truckNumber: this.form.value.truckNumber!,
      truckType: this.form.value.truckType!,
      truckStatus: this.form.value.truckStatus!,
      mainDriverId: this.form.value.mainDriver?.id,
      secondaryDriverId: this.form.value.secondaryDriver?.id,
    };

    this.apiService.truckApi.createTruck(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new truck has been created successfully");
        this.router.navigateByUrl("/trucks");
      }

      this.isLoading.set(false);
    });
  }

  private updateTruck(): void {
    this.isLoading.set(true);

    const updateTruckCommand: UpdateTruckCommand = {
      id: this.id()!,
      truckNumber: this.form.value.truckNumber,
      truckType: this.form.value.truckType,
      truckStatus: this.form.value.truckStatus,
      mainDriverId: this.form.value.mainDriver?.id,
      secondaryDriverId: this.form.value.secondaryDriver?.id,
    };

    this.apiService.truckApi.updateTruck(updateTruckCommand).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Truck has been updated successfully");
      }

      this.isLoading.set(false);
    });
  }

  private deleteTruck(): void {
    if (!this.id()) {
      return;
    }

    this.isLoading.set(true);
    this.apiService.truckApi.deleteTruck(this.id()!).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A truck has been deleted successfully");
        this.router.navigateByUrl("/trucks");
      }

      this.isLoading.set(false);
    });
  }
}
