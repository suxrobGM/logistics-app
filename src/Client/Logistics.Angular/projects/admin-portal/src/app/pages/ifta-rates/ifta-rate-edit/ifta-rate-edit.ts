import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { ActivatedRoute, Router, RouterModule } from "@angular/router";
import { ToastService } from "@logistics/shared";
import {
  Api,
  deleteIftaTaxRate,
  getIftaTaxRateById,
  updateIftaTaxRate,
  type IftaTaxRateDto,
  type UpdateIftaTaxRateCommand,
} from "@logistics/shared/api";
import { Card, PageHeader, Skeleton, Stack } from "@logistics/shared/ui";
import { IftaRateForm, type IftaRateFormValue } from "@/shared/components";

@Component({
  selector: "adm-ifta-rate-edit",
  templateUrl: "./ifta-rate-edit.html",
  imports: [Card, IftaRateForm, PageHeader, RouterModule, Skeleton, Stack],
})
export class IftaRateEdit implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);
  private readonly router = inject(Router);
  private readonly route = inject(ActivatedRoute);

  protected readonly isLoading = signal<boolean>(false);
  protected readonly isFetching = signal<boolean>(true);
  protected readonly rate = signal<IftaTaxRateDto | null>(null);

  ngOnInit(): void {
    this.fetchRate();
  }

  private async fetchRate(): Promise<void> {
    const id = this.route.snapshot.paramMap.get("id");
    if (!id) {
      this.router.navigateByUrl("/ifta-rates");
      return;
    }

    this.isFetching.set(true);
    const rate = await this.api.invoke(getIftaTaxRateById, { id });

    if (!rate) {
      this.toastService.showError("IFTA tax rate not found");
      this.router.navigateByUrl("/ifta-rates");
      return;
    }

    this.rate.set(rate);
    this.isFetching.set(false);
  }

  protected readonly initialValue = computed<Partial<IftaRateFormValue> | null>(() => {
    const rate = this.rate();
    if (!rate) return null;

    return {
      countryCode: (rate.jurisdiction?.countryCode as "US" | "CA") ?? "US",
      region: rate.jurisdiction?.region ?? "",
      year: rate.year,
      quarter: rate.quarter,
      ratePerGallon: rate.ratePerGallon ?? null,
      surchargePerGallon: rate.surchargeRatePerGallon ?? null,
    };
  });

  protected async onSave(formValue: IftaRateFormValue): Promise<void> {
    const rate = this.rate();
    if (!rate) return;

    this.isLoading.set(true);

    const command: UpdateIftaTaxRateCommand = {
      id: rate.id!,
      countryCode: formValue.countryCode,
      region: formValue.region || undefined,
      year: formValue.year,
      quarter: formValue.quarter,
      ratePerGallon: formValue.ratePerGallon ?? 0,
      surchargeRatePerGallon: formValue.surchargePerGallon ?? undefined,
    };

    await this.api.invoke(updateIftaTaxRate, { id: rate.id!, body: command });
    this.toastService.showSuccess("IFTA tax rate has been updated successfully");
    this.router.navigateByUrl("/ifta-rates");

    this.isLoading.set(false);
  }

  protected async onRemove(): Promise<void> {
    const rate = this.rate();
    if (!rate) return;

    await this.api.invoke(deleteIftaTaxRate, { id: rate.id! });
    this.toastService.showSuccess("IFTA tax rate has been deleted successfully");
    this.router.navigateByUrl("/ifta-rates");
  }
}
