import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { RouterLink } from "@angular/router";
import { type FuelCardProviderConfigurationDto } from "@logistics/shared/api";
import { RelativeTimePipe } from "@logistics/shared/pipes";
import { Badge, Icon, Stack, UiButton, UiDataTable, UiTooltip } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { getFuelCardProviderLabel } from "../fuel-cards.constants";

@Component({
  selector: "app-fuel-card-providers-table",
  templateUrl: "./providers-table.html",
  imports: [
    Badge,
    DatePipe,
    Icon,
    RelativeTimePipe,
    RouterLink,
    Stack,
    UiButton,
    UiDataTable,
    UiTooltip,
  ],
})
export class FuelCardProvidersTable {
  private readonly toast = inject(ToastService);

  public readonly providers = input.required<FuelCardProviderConfigurationDto[]>();
  /** Id of the provider currently syncing (from the parent), so only that row shows a spinner. */
  public readonly syncingId = input<string | null>(null);
  public readonly sync = output<FuelCardProviderConfigurationDto>();
  public readonly delete = output<string>();

  protected readonly getProviderLabel = getFuelCardProviderLabel;

  protected confirmDelete(provider: FuelCardProviderConfigurationDto): void {
    this.toast.confirm({
      message: `Are you sure you want to delete the ${getFuelCardProviderLabel(provider.providerType)} provider? Already-imported transactions and expenses are kept.`,
      header: "Delete Provider",
      icon: "warning",
      severity: "danger",
      accept: () => this.delete.emit(provider.id!),
    });
  }
}
