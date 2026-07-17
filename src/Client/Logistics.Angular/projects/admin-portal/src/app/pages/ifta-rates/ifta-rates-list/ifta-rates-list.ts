import { DecimalPipe } from "@angular/common";
import { Component, computed, inject, signal } from "@angular/core";
import { Router } from "@angular/router";
import { Api, deleteIftaTaxRate, type IftaTaxRateDto } from "@logistics/shared/api";
import {
  Card,
  DataContainer,
  PageHeader,
  SearchField,
  Stack,
  UiButton,
  UiDataTable,
  UiMenu,
  UiSelectField,
  UiSortHeader,
  type UiMenuItem,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { IftaRatesListStore } from "../store/ifta-rates-list.store";

const QUARTER_OPTIONS = [1, 2, 3, 4].map((q) => ({ label: `Q${q}`, value: q }));

const CURRENT_YEAR = new Date().getFullYear();
const YEAR_OPTIONS = Array.from({ length: 5 }, (_, i) => {
  const year = CURRENT_YEAR + 2 - i;
  return { label: `${year}`, value: year };
});

@Component({
  selector: "adm-ifta-rates-list",
  templateUrl: "./ifta-rates-list.html",
  providers: [IftaRatesListStore],
  imports: [
    Card,
    DataContainer,
    DecimalPipe,
    PageHeader,
    SearchField,
    Stack,
    UiButton,
    UiDataTable,
    UiMenu,
    UiSelectField,
    UiSortHeader,
  ],
})
export class IftaRatesList {
  private readonly api = inject(Api);
  private readonly router = inject(Router);
  private readonly toastService = inject(ToastService);
  protected readonly store = inject(IftaRatesListStore);

  protected readonly yearOptions = YEAR_OPTIONS;
  protected readonly quarterOptions = QUARTER_OPTIONS;

  protected readonly yearFilter = signal<number | null>(null);
  protected readonly quarterFilter = signal<number | null>(null);

  protected readonly selectedRate = signal<IftaTaxRateDto | null>(null);

  protected readonly actionMenuItems = computed<UiMenuItem[]>(() => {
    const rate = this.selectedRate();
    if (!rate) return [];

    return [
      {
        label: "Edit",
        icon: "square-pen",
        command: () => this.editRate(),
      },
      { separator: true },
      {
        label: "Delete",
        icon: "trash",
        variant: "destructive",
        command: () => this.confirmDelete(),
      },
    ];
  });

  protected openActionMenu(
    rate: IftaTaxRateDto,
    menu: { toggle: (event: Event) => void },
    event: Event,
  ): void {
    this.selectedRate.set(rate);
    menu.toggle(event);
  }

  protected search(value: string): void {
    this.store.setSearch(value.toUpperCase());
  }

  protected onYearChange(year: number | null): void {
    this.yearFilter.set(year);
    this.applyFilters();
  }

  protected onQuarterChange(quarter: number | null): void {
    this.quarterFilter.set(quarter);
    this.applyFilters();
  }

  protected addRate(): void {
    this.router.navigate(["/ifta-rates/add"]);
  }

  protected jurisdictionLabel(rate: IftaTaxRateDto): string {
    const jurisdiction = rate.jurisdiction;
    if (!jurisdiction?.countryCode) return "-";
    return jurisdiction.region
      ? `${jurisdiction.countryCode}-${jurisdiction.region}`
      : jurisdiction.countryCode;
  }

  private editRate(): void {
    const rate = this.selectedRate();
    if (rate) {
      this.router.navigate(["/ifta-rates", rate.id, "edit"]);
    }
  }

  private confirmDelete(): void {
    const rate = this.selectedRate();
    if (!rate?.id) return;

    this.toastService.confirmDelete("IFTA tax rate", () => this.deleteRate(rate.id!));
  }

  private async deleteRate(id: string): Promise<void> {
    await this.api.invoke(deleteIftaTaxRate, { id });
    this.toastService.showSuccess("The IFTA tax rate has been deleted successfully");
    this.store.removeItem(id);
  }

  private applyFilters(): void {
    this.store.setFilters({
      Year: this.yearFilter() ?? undefined,
      Quarter: this.quarterFilter() ?? undefined,
    });
  }
}
