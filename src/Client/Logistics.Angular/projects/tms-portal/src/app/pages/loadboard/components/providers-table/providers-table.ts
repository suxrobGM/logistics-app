import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { type LoadBoardConfigurationDto, type LoadBoardProviderType } from "@logistics/shared/api";
import { Badge, Icon, Stack, UiButton, UiDataTable, UiTooltip } from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { getProviderLabel } from "../loadboard.constants";

@Component({
  selector: "app-providers-table",
  templateUrl: "./providers-table.html",
  imports: [Badge, DatePipe, Icon, Stack, UiButton, UiDataTable, UiTooltip],
})
export class ProvidersTable {
  private readonly toast = inject(ToastService);

  public readonly providers = input.required<LoadBoardConfigurationDto[]>();
  public readonly delete = output<string>();

  protected readonly getProviderLabel = getProviderLabel;

  protected confirmDelete(provider: LoadBoardConfigurationDto): void {
    this.toast.confirm({
      message: `Are you sure you want to delete the ${getProviderLabel(provider.providerType as LoadBoardProviderType)} provider?`,
      header: "Delete Provider",
      icon: "warning",
      severity: "danger",
      accept: () => this.delete.emit(provider.id!),
    });
  }
}
