import { Component, inject, signal, type OnInit } from "@angular/core";
import {
  Api,
  createFuelCardProvider,
  deleteFuelCardProvider,
  getFuelCardProviders,
  syncFuelCardTransactions,
  type CreateFuelCardProviderConfigurationCommand,
  type FuelCardProviderConfigurationDto,
  type FuelCardSyncResultDto,
} from "@logistics/shared/api";
import {
  Alert,
  DashboardCard,
  EmptyState,
  ErrorState,
  Spinner,
  Stack,
  UiButton,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { PageHeader } from "@/shared/components";
import { FuelCardProviderAddDialog, FuelCardProvidersTable } from "../components";

@Component({
  selector: "app-fuel-card-providers",
  templateUrl: "./fuel-card-providers.html",
  imports: [
    Alert,
    DashboardCard,
    EmptyState,
    ErrorState,
    FuelCardProviderAddDialog,
    FuelCardProvidersTable,
    PageHeader,
    Spinner,
    Stack,
    UiButton,
  ],
})
export class FuelCardProvidersComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toast = inject(ToastService);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly syncingAll = signal(false);
  protected readonly syncingId = signal<string | null>(null);
  protected readonly error = signal<string | null>(null);
  protected readonly providers = signal<FuelCardProviderConfigurationDto[]>([]);
  protected readonly showAddDialog = signal(false);

  ngOnInit(): void {
    void this.loadProviders();
  }

  protected async loadProviders(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const data = await this.api.invoke(getFuelCardProviders);
      this.providers.set(data ?? []);
    } catch (err) {
      this.error.set("Failed to load fuel card providers");
      console.error("Error loading providers:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSave(command: CreateFuelCardProviderConfigurationCommand): Promise<void> {
    this.saving.set(true);
    try {
      await this.api.invoke(createFuelCardProvider, { body: command });
      this.showAddDialog.set(false);
      this.toast.showSuccess("Fuel card provider added");
      await this.loadProviders();
    } catch (err) {
      console.error("Error saving provider:", err);
      this.toast.showError("Failed to add provider. Verify the API credentials.");
    } finally {
      this.saving.set(false);
    }
  }

  protected async onSync(provider: FuelCardProviderConfigurationDto): Promise<void> {
    this.syncingId.set(provider.id ?? null);
    try {
      const result = await this.api.invoke(syncFuelCardTransactions, {
        body: { providerType: provider.providerType },
      });
      this.showSyncResult(result);
      await this.loadProviders();
    } catch (err) {
      console.error("Error syncing transactions:", err);
      this.toast.showError("Failed to sync transactions");
    } finally {
      this.syncingId.set(null);
    }
  }

  /** Syncs every connected provider in one call (no provider type = all). */
  protected async syncAll(): Promise<void> {
    this.syncingAll.set(true);
    try {
      const result = await this.api.invoke(syncFuelCardTransactions, { body: {} });
      this.showSyncResult(result);
      await this.loadProviders();
    } catch (err) {
      console.error("Error syncing transactions:", err);
      this.toast.showError("Failed to sync transactions");
    } finally {
      this.syncingAll.set(false);
    }
  }

  private showSyncResult(result: FuelCardSyncResultDto | null): void {
    this.toast.showSuccess(
      `Imported ${result?.imported ?? 0} transaction(s): ${result?.matched ?? 0} matched, ${result?.pending ?? 0} pending review.`,
      "Sync Complete",
    );
  }

  protected async deleteProvider(providerId: string): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(deleteFuelCardProvider, { providerId });
      await this.loadProviders();
    } catch (err) {
      console.error("Error deleting provider:", err);
      this.error.set("Failed to delete provider");
    } finally {
      this.loading.set(false);
    }
  }
}
