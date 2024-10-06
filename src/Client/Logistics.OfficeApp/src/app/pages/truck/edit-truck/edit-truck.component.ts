import {Component, OnInit} from "@angular/core";
import {NgIf} from "@angular/common";
import {FormControl, FormGroup, Validators, FormsModule, ReactiveFormsModule} from "@angular/forms";
import {ActivatedRoute, RouterLink} from "@angular/router";
import {ConfirmationService} from "primeng/api";
import {ButtonModule} from "primeng/button";
import {AutoCompleteModule} from "primeng/autocomplete";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {CardModule} from "primeng/card";
import {ConfirmDialogModule} from "primeng/confirmdialog";
import {ToastModule} from "primeng/toast";
import {CreateTruckCommand, EmployeeDto, UpdateTruckCommand} from "@/core/models";
import {ApiService, ToastService} from "@/core/services";

@Component({
  selector: "app-edit-truck",
  templateUrl: "./edit-truck.component.html",
  styleUrls: ["./edit-truck.component.scss"],
  standalone: true,
  imports: [
    ToastModule,
    ConfirmDialogModule,
    CardModule,
    NgIf,
    ProgressSpinnerModule,
    FormsModule,
    ReactiveFormsModule,
    AutoCompleteModule,
    ButtonModule,
    RouterLink,
  ],
  providers: [ConfirmationService],
})
export class EditTruckComponent implements OnInit {
  public id: string | null;
  public headerText: string;
  public isBusy: boolean;
  public editMode: boolean;
  public form: FormGroup;
  public suggestedDrivers: EmployeeDto[];

  constructor(
    private apiService: ApiService,
    private confirmationService: ConfirmationService,
    private toastService: ToastService,
    private route: ActivatedRoute
  ) {
    this.id = null;
    this.suggestedDrivers = [];
    this.headerText = "Edit a truck";
    this.isBusy = false;
    this.editMode = true;
    this.form = new FormGroup({
      truckNumber: new FormControl(0, Validators.required),
      drivers: new FormControl([], Validators.required),
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
    });

    if (!this.id) {
      this.editMode = false;
      this.headerText = "Add a new truck";
      return;
    }

    this.fetchTruck(this.id);
  }

  searchDriver(event: {query: string}) {
    this.apiService.getDrivers({search: event.query}).subscribe((result) => {
      if (result.success && result.data) {
        this.suggestedDrivers = result.data;
      }
    });
  }

  submit() {
    const drivers = this.form.value.drivers as EmployeeDto[];

    if (drivers.length === 0) {
      this.toastService.showError("Select a driver");
      return;
    }

    if (this.editMode) {
      this.updateTruck();
    } else {
      this.createTruck();
    }
  }

  confirmToDelete() {
    this.confirmationService.confirm({
      message: "Are you sure that you want to delete this truck?",
      accept: () => this.deleteTruck(),
    });
  }

  private fetchTruck(id: string) {
    this.apiService.getTruck(id).subscribe((result) => {
      if (result.success && result.data) {
        const truck = result.data;
        this.form.patchValue({
          truckNumber: truck.truckNumber,
          drivers: truck.drivers,
        });
      }
    });
  }

  private createTruck() {
    this.isBusy = true;
    const drivers = this.form.value.drivers as EmployeeDto[];

    const command: CreateTruckCommand = {
      truckNumber: this.form.value.truckNumber,
      driverIds: drivers.map((i) => i.id),
    };

    this.apiService.createTruck(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new truck has been created successfully");
        this.resetForm();
      }

      this.isBusy = false;
    });
  }

  private updateTruck() {
    this.isBusy = true;
    const drivers = this.form.value.drivers as EmployeeDto[];

    const updateTruckCommand: UpdateTruckCommand = {
      id: this.id!,
      truckNumber: this.form.value.truckNumber,
      driverIds: drivers.map((i) => i.id),
    };

    this.apiService.updateTruck(updateTruckCommand).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("Truck has been updated successfully");
      }

      this.isBusy = false;
    });
  }

  private deleteTruck() {
    if (!this.id) {
      return;
    }

    this.isBusy = true;
    this.apiService.deleteTruck(this.id).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A truck has been deleted successfully");
        this.resetForm();
      }

      this.isBusy = false;
    });
  }

  private resetForm() {
    this.editMode = false;
    this.headerText = "Add a new truck";
    this.id = null;
  }
}
