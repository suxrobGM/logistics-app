import {CommonModule} from "@angular/common";
import {Component, OnInit, inject, input, signal} from "@angular/core";
import {FormControl, FormGroup, ReactiveFormsModule, Validators} from "@angular/forms";
import {Router, RouterModule} from "@angular/router";
import {ButtonModule} from "primeng/button";
import {CardModule} from "primeng/card";
import {ProgressSpinnerModule} from "primeng/progressspinner";
import {ApiService} from "@/core/api";
import {UpdateCustomerCommand} from "@/core/api/models";
import {ToastService} from "@/core/services";
import {ValidationSummary} from "@/shared/components";

@Component({
  selector: "app-edit-customer",
  templateUrl: "./edit-customer.html",
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
  private readonly router = inject(Router);

  protected readonly form: FormGroup<CustomerForm>;
  protected readonly id = input<string>();
  protected readonly title = signal<string>("Edit customer");
  protected readonly isLoading = signal<boolean>(false);

  constructor() {
    this.form = new FormGroup<CustomerForm>({
      name: new FormControl<string>("", {validators: Validators.required, nonNullable: true}),
    });
  }

  ngOnInit(): void {
    if (this.id()) {
      this.title.set("Edit customer");
    } else {
      this.title.set("Add a new customer");
    }
  }

  submit(): void {
    if (!this.form.valid) {
      return;
    }

    if (this.id()) {
      this.updateCustomer();
    } else {
      this.addCustomer();
    }
  }

  private addCustomer(): void {
    this.isLoading.set(true);

    this.apiService.createCustomer({name: this.form.value.name!}).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A new customer has been added successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading.set(false);
    });
  }

  private updateCustomer(): void {
    this.isLoading.set(true);

    const command: UpdateCustomerCommand = {
      id: this.id()!,
      name: this.form.value.name!,
    };

    this.apiService.updateCustomer(command).subscribe((result) => {
      if (result.success) {
        this.toastService.showSuccess("A customer data has been updated successfully");
        this.router.navigateByUrl("/customers");
      }

      this.isLoading.set(false);
    });
  }
}

interface CustomerForm {
  name: FormControl<string>;
}
