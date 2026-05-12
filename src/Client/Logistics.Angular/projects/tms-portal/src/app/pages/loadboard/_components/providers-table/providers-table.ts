import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { type LoadBoardConfigurationDto, type LoadBoardProviderType } from "@logistics/shared/api";
import { Icon, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { getProviderLabel } from "../loadboard.constants";

@Component({
  selector: "app-providers-table",
  templateUrl: "./providers-table.html",
  imports: [ButtonModule, DatePipe, Icon, Stack, TableModule, TagModule, TooltipModule],
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
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.delete.emit(provider.id!),
    });
  }
}
