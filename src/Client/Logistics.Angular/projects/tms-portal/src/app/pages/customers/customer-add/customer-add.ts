import { Component, inject } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import type { CustomerDto } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CustomerForm, PageHeader } from "@/shared/components";

@Component({
  selector: "app-customer-add",
  templateUrl: "./customer-add.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm, PageHeader],
})
export class CustomerAddComponent {
  private readonly router = inject(Router);

  protected handleCustomerCreated(_customer: CustomerDto): void {
    this.router.navigateByUrl("/customers");
  }
}
