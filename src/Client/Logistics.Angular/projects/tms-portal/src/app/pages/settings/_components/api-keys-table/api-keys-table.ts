import { DatePipe } from "@angular/common";
import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  Api,
  createApiKey,
  getApiKeys,
  revokeApiKey,
  type ApiKeyCreatedDto,
  type ApiKeyDto,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { InputTextModule } from "primeng/inputtext";
import { MessageModule } from "primeng/message";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TableModule } from "primeng/table";
import { TooltipModule } from "primeng/tooltip";
import { ToastService } from "@/core/services";
import { FormField } from "@/shared/components";

@Component({
  selector: "app-api-keys-table",
  templateUrl: "./api-keys-table.html",
  imports: [
    FormsModule,
    ButtonModule,
    DialogModule,
    InputTextModule,
    MessageModule,
    TableModule,
    TooltipModule,
    FormField,
    DatePipe,
    ProgressSpinnerModule,
  ],
})
export class ApiKeysTable implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isCreating = signal(false);
  protected readonly apiKeys = signal<ApiKeyDto[]>([]);

  // Create dialog
  protected readonly showCreateDialog = signal(false);
  protected readonly newKeyName = signal("");

  // One-time key display dialog
  protected readonly showKeyDialog = signal(false);
  protected readonly createdKey = signal<ApiKeyCreatedDto | null>(null);

  ngOnInit(): void {
    this.loadApiKeys();
  }

  private async loadApiKeys(): Promise<void> {
    this.isLoading.set(true);
    try {
      const keys = await this.api.invoke(getApiKeys);
      this.apiKeys.set(keys);
    } catch {
      this.toastService.showError("Failed to load API keys");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openCreateDialog(): void {
    this.newKeyName.set("");
    this.showCreateDialog.set(true);
  }

  protected async createKey(): Promise<void> {
    const name = this.newKeyName().trim();
    if (!name) return;

    this.isCreating.set(true);
    try {
      const result = await this.api.invoke(createApiKey, { body: { name } });
      this.showCreateDialog.set(false);
      this.createdKey.set(result);
      this.showKeyDialog.set(true);
      await this.loadApiKeys();
      this.toastService.showSuccess("API key created successfully");
    } catch {
      this.toastService.showError("Failed to create API key");
    } finally {
      this.isCreating.set(false);
    }
  }

  protected confirmRevoke(key: ApiKeyDto): void {
    this.toastService.confirmDelete("API key", () => this.revokeKey(key.id!));
  }

  private async revokeKey(id: string): Promise<void> {
    try {
      await this.api.invoke(revokeApiKey, { id });
      this.toastService.showSuccess("API key revoked successfully");
      await this.loadApiKeys();
    } catch {
      this.toastService.showError("Failed to revoke API key");
    }
  }

  protected async copyToClipboard(text: string | null | undefined): Promise<void> {
    if (!text) return;
    try {
      await navigator.clipboard.writeText(text);
      this.toastService.showSuccess("Copied to clipboard");
    } catch {
      this.toastService.showError("Failed to copy to clipboard");
    }
  }

  protected onKeyDialogHide(): void {
    this.createdKey.set(null);
  }
}
