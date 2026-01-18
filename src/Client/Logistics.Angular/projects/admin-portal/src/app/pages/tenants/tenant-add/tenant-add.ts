import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { Api, createTenant } from "@logistics/shared/api";
import type { CreateTenantCommand } from "@logistics/shared/api/models";
import { ToastService } from "@logistics/shared";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { DividerModule } from "primeng/divider";
import { TenantForm, type TenantFormValue } from "@/shared/components";

@Component({
  selector: "adm-tenant-add",
  templateUrl: "./tenant-add.html",
  imports: [CardModule, ButtonModule, RouterModule, TenantForm, DividerModule],
})
export class TenantAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);

  protected async onSave(formValue: TenantFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateTenantCommand = {
      name: formValue.name,
      companyName: formValue.companyName,
      billingEmail: formValue.billingEmail,
      dotNumber: formValue.dotNumber || undefined,
      companyAddress: formValue.companyAddress ?? undefined,
    };

    await this.api.invoke(createTenant, { body: command });
    this.toastService.showSuccess("Tenant has been created successfully");
    this.router.navigateByUrl("/tenants");

    this.isLoading.set(false);
  }
}
