import {Component, OnInit, computed, inject, input, signal} from "@angular/core";
import {FormControl, FormGroup, FormsModule, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {AutoCompleteModule} from "primeng/autocomplete";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ToastModule} from "primeng/toast";
import {ApiService} from "@/core/api";
import {CreateTruckCommand, EmployeeDto, UpdateTruckCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";

@Component({
  selector: "app-edit-truck",
  templateUrl: "./edit-truck.html",
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
  ],
})
export class EditTruckComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly confirmationService = inject(ConfirmationService);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly form: FormGroup;

  protected readonly id = input<string | null>(null);
  protected readonly isLoading = signal(false);
  protected readonly suggestedDrivers = signal<EmployeeDto[]>([]);
  protected readonly title = computed(() => (this.id() ? "Edit a truck" : "Add a new truck"));

  constructor() {
    this.form = new FormGroup({
      truckNumber: new FormControl(0, Validators.required),
      drivers: new FormControl([], Validators.required),
    });
  }

  ngOnInit(): void {
    const id = this.id();

    if (id) {
      this.fetchTruck(id);
    }
  }

  protected searchDriver(event: {query: string}): void {
    this.apiService.getDrivers({search: event.query}).subscribe((result) => {
      if (result.success && result.data) {
        this.suggestedDrivers.set(result.data);
      }
    });
  }

  protected submit(): void {
    const drivers = this.form.value.drivers as EmployeeDto[];

    if (drivers.length === 0) {
      this.toastService.showError("Select a driver");
      return;
    }

    if (this.id()) {
      this.updateTruck();
    } else {
      this.createTruck();
    }
  }

  protected confirmToDelete(): void {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this truck?",
      accept: () => this.deleteTruck(),
    });
  }

  private fetchTruck(id: string): void {
    this.apiService.getTruck(id).subscribe((result) => {
      if (result.success && result.data) {
        const truck = result.data;
        console.log("Fetched Truck:", truck);

        this.form.patchValue({
          truckNumber: truck.truckNumber,
          drivers: truck.drivers,
        });
      }
    });
  }

  private createTruck(): void {
    this.isLoading.set(true);
    const drivers = this.form.value.drivers as EmployeeDto[];

    const command: CreateTruckCommand = {
      truckNumber: this.form.value.truckNumber,
      driverIds: drivers.map((i) => i.id),
    };

    this.apiService.createTruck(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new truck has been created successfully");
        this.router.navigateByUrl("/trucks");
      }

      this.isLoading.set(false);
    });
  }

  private updateTruck(): void {
    this.isLoading.set(true);
    const drivers = this.form.value.drivers as EmployeeDto[];

    const updateTruckCommand: UpdateTruckCommand = {
      id: this.id()!,
      truckNumber: this.form.value.truckNumber,
      driverIds: drivers.map((i) => i.id),
    };

    this.apiService.updateTruck(updateTruckCommand).subscribe((result) => {
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
    this.apiService.deleteTruck(this.id()!).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A truck has been deleted successfully");
        this.router.navigateByUrl("/trucks");
      }

      this.isLoading.set(false);
    });
  }
}
