import { Component, inject, signal } from "@angular/core";
import { Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import { Api, createIftaTaxRate, type CreateIftaTaxRateCommand } from "@logistics/shared/api";
import { Card, PageHeader } from "@logistics/shared/ui";
import { IftaRateForm, type IftaRateFormValue } from "@/shared/components";

@Component({
  selector: "adm-ifta-rate-add",
  templateUrl: "./ifta-rate-add.html",
  imports: [Card, IftaRateForm, PageHeader, RouterModule],
})
export class IftaRateAdd {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);

  protected readonly isLoading = signal<boolean>(false);

  protected async onSave(formValue: IftaRateFormValue): Promise<void> {
    this.isLoading.set(true);

    const command: CreateIftaTaxRateCommand = {
      countryCode: formValue.countryCode,
      region: formValue.region || undefined,
      year: formValue.year,
      quarter: formValue.quarter,
      ratePerGallon: formValue.ratePerGallon ?? 0,
      surchargeRatePerGallon: formValue.surchargePerGallon ?? undefined,
    };

    await this.api.invoke(createIftaTaxRate, { body: command });
    this.toastService.showSuccess("IFTA tax rate has been created successfully");
    this.router.navigateByUrl("/ifta-rates");

    this.isLoading.set(false);
  }
}
