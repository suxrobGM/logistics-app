import { Component, inject, signal, type OnInit } from "@angular/core";
import { Router } from "@angular/router";
import {
  Api,
  createEldProvider,
  deleteEldProvider,
  getEldProviders,
  type CreateEldProviderConfigurationCommand,
  type EldProviderConfigurationDto,
} from "@logistics/shared/api";
import { Alert, DashboardCard, EmptyState, ErrorState, Stack } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { PageHeader } from "@/shared/components";
import { EldProviderAddDialog, EldProvidersTable } from "../_components";

@Component({
  selector: "app-eld-providers",
  templateUrl: "./eld-providers.html",
  imports: [
    Alert,
    ButtonModule,
    DashboardCard,
    EldProviderAddDialog,
    EldProvidersTable,
    EmptyState,
    ErrorState,
    PageHeader,
    ProgressSpinnerModule,
    Stack,
  ],
})
export class EldProvidersComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly router = inject(Router);

  protected readonly loading = signal(true);
  protected readonly saving = signal(false);
  protected readonly error = signal<string | null>(null);
  protected readonly providers = signal<EldProviderConfigurationDto[]>([]);
  protected readonly showAddDialog = signal(false);

  ngOnInit(): void {
    this.loadProviders();
  }

  protected async loadProviders(): Promise<void> {
    this.loading.set(true);
    this.error.set(null);

    try {
      const data = await this.api.invoke(getEldProviders);
      this.providers.set(data ?? []);
    } catch (err) {
      this.error.set("Failed to load ELD providers");
      console.error("Error loading providers:", err);
    } finally {
      this.loading.set(false);
    }
  }

  protected async onSave(command: CreateEldProviderConfigurationCommand): Promise<void> {
    this.saving.set(true);
    try {
      await this.api.invoke(createEldProvider, { body: command });
      this.showAddDialog.set(false);
      await this.loadProviders();
    } catch (err) {
      console.error("Error saving provider:", err);
    } finally {
      this.saving.set(false);
    }
  }

  protected manageMappings(providerId: string): void {
    this.router.navigate(["/eld/providers", providerId, "mappings"]);
  }

  protected async deleteProvider(providerId: string): Promise<void> {
    this.loading.set(true);
    try {
      await this.api.invoke(deleteEldProvider, { providerId });
      await this.loadProviders();
    } catch (err) {
      console.error("Error deleting provider:", err);
      this.error.set("Failed to delete provider");
    } finally {
      this.loading.set(false);
    }
  }
}
