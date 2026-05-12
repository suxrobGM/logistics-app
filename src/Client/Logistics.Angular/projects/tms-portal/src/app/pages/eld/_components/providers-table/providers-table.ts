import { DatePipe } from "@angular/common";
import { Component, inject, input, output } from "@angular/core";
import { type EldProviderConfigurationDto } from "@logistics/shared/api";
import { Icon, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { TableModule } from "primeng/table";
import { TagModule } from "primeng/tag";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { getEldProviderLabel } from "../eld.constants";

@Component({
  selector: "app-eld-providers-table",
  templateUrl: "./providers-table.html",
  imports: [ButtonModule, DatePipe, Icon, Stack, TableModule, TagModule, TooltipModule],
})
export class EldProvidersTable {
  private readonly toast = inject(ToastService);

  public readonly providers = input.required<EldProviderConfigurationDto[]>();
  public readonly manageMappings = output<string>();
  public readonly delete = output<string>();

  protected readonly getProviderLabel = getEldProviderLabel;

  protected confirmDelete(provider: EldProviderConfigurationDto): void {
    this.toast.confirm({
      message: `Are you sure you want to delete the ${getEldProviderLabel(provider.providerType)} provider? This will also remove all driver mappings and HOS data.`,
      header: "Delete Provider",
      icon: "pi pi-exclamation-triangle",
      acceptButtonStyleClass: "p-button-danger",
      accept: () => this.delete.emit(provider.id!),
    });
  }
}
