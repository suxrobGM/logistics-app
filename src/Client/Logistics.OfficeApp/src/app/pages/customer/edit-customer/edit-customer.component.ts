import {CommonModule} from "@angular/common";
import {Component, OnInit, inject} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {ActivatedRoute, Router, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ApiService} from "@/core/api";
import {UpdateCustomerCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {ValidationSummary} from "@/shared/components";

@Component({
  selector: "app-edit-customer",
  standalone: true,
  templateUrl: "./edit-customer.component.html",
  styleUrls: [],
  imports: [
    CommonModule,
    CardModule,
    ButtonModule,
    ProgressSpinnerModule,
    ReactiveFormsModule,
    RouterModule,
    ValidationSummary,
  ],
})
export class EditCustomerComponent implements OnInit {
  private readonly apiService = inject(ApiService);
  private readonly toastService = inject(ToastService);
  private readonly route = inject(ActivatedRoute);
  private readonly router = inject(Router);

  public title: string;
  public id: string | null;
  public form: FormGroup<CustomerForm>;
  public isLoading: boolean;

  constructor() {
    this.title = "Edit customer";
    this.id = null;
    this.isLoading = false;

    this.form = new FormGroup<CustomerForm>({
      name: new FormControl<string>("", {validators: Validators.required, nonNullable: true}),
    });
  }

  ngOnInit(): void {
    this.route.params.subscribe((params) => {
      this.id = params["id"];
    });

    if (this.id) {
      this.title = "Edit customer";
      this.fetchCustomer();
    } else {
      this.title = "Add a new customer";
    }
  }

  submit() {
    if (!this.form.valid) {
      return;
    }

    if (this.id) {
      this.updateCustomer();
    } else {
      this.addCustomer();
    }
  }

  private fetchCustomer() {
    this.isLoading = true;

    this.apiService.getCustomer(this.id!).subscribe((result) => {
      if (result.success) {
        this.form.patchValue({name: result.data?.name});
      }

      this.isLoading = false;
    });
  }

  private addCustomer() {
    this.isLoading = true;

    this.apiService.createCustomer({name: this.form.value.name!}).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new customer has been added successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading = false;
    });
  }

  private updateCustomer() {
    this.isLoading = true;

    const commad: UpdateCustomerCommand = {
      id: this.id!,
      name: this.form.value.name!,
    };

    this.apiService.updateCustomer(commad).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A customer data has been updated successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading = false;
    });
  }
}

interface CustomerForm {
  name: FormControl<string>;
}
