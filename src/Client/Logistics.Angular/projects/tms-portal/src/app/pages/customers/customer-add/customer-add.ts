import { Component, inject } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import type { CustomerDto } from "@logistics/shared/api";
import { Container } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CustomerForm, PageHeader } from "@/shared/components";

@Component({
  selector: "app-customer-add",
  templateUrl: "./customer-add.html",
  imports: [CardModule, ButtonModule, RouterModule, CustomerForm, PageHeader, Container],
})
export class CustomerAddComponent {
  private readonly router = inject(Router);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  protected handleCustomerCreated(_customer: CustomerDto): void {
    this.router.navigateByUrl("/customers");
  }
}
