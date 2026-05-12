import { Component, inject, signal, type OnInit } from "@angular/core";
import { type CreateLoadBoardConfigurationCommand } from "@logistics/shared/api";
import { Alert, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { DashboardCard, EmptyState, ErrorState, PageHeader } from "@/shared/components";
import { ProviderAddDialog, ProvidersTable } from "../_components";
import { LoadBoardStore } from "../store";

@Component({
  selector: "app-loadboard-providers",
  templateUrl: "./loadboard-providers.html",
  imports: [
    Alert,
    ButtonModule,
    DashboardCard,
    EmptyState,
    ErrorState,
    PageHeader,
    ProgressSpinnerModule,
    ProviderAddDialog,
    ProvidersTable,
    Stack,
  ],
})
export class LoadBoardProvidersComponent implements OnInit {
  protected readonly store = inject(LoadBoardStore);

  protected readonly showAddDialog = signal(false);
  protected readonly saving = signal(false);

  ngOnInit(): void {
    void this.store.loadProviders();
  }

  protected async onSave(body: CreateLoadBoardConfigurationCommand): Promise<void> {
    this.saving.set(true);
    const ok = await this.store.createProvider(body);
    this.saving.set(false);
    if (ok) this.showAddDialog.set(false);
  }
}
