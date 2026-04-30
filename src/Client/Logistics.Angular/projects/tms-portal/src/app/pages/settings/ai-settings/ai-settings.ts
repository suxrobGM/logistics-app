import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  Api,
  getAiQuotaStatus,
  updateTenantAiSettings,
  type AiQuotaStatusDto,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { MessageModule } from "primeng/message";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TagModule } from "primeng/tag";
import { ToastService } from "@/core/services";
import { AiQuotaUsage, FormField } from "@/shared/components";

interface ModelOption {
  label: string;
  value: string;
  multiplier: number;
  tier: "base" | "premium" | "ultra";
}

const allModels: ModelOption[] = [
  { label: "Claude Haiku 4.5 (default)", value: "", multiplier: 1, tier: "base" },
  { label: "DeepSeek Chat", value: "deepseek-chat", multiplier: 1, tier: "base" },
  { label: "DeepSeek Reasoner", value: "deepseek-reasoner", multiplier: 1, tier: "base" },
  { label: "GPT-5.4 Mini", value: "gpt-5.4-mini", multiplier: 1, tier: "base" },
  { label: "Claude Haiku 4.5", value: "claude-haiku-4-5", multiplier: 1, tier: "base" },
  { label: "GPT-5.4", value: "gpt-5.4", multiplier: 5, tier: "premium" },
  { label: "Claude Sonnet 4.6", value: "claude-sonnet-4-6", multiplier: 5, tier: "premium" },
  { label: "Claude Opus 4.6", value: "claude-opus-4-6", multiplier: 10, tier: "ultra" },
];

@Component({
  selector: "app-ai-settings",
  templateUrl: "./ai-settings.html",
  imports: [
    FormsModule,
    CardModule,
    ProgressSpinnerModule,
    ButtonModule,
    SelectModule,
    TagModule,
    MessageModule,
    FormField,
    AiQuotaUsage,
  ],
})
export class AiSettingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly quotaStatus = signal<AiQuotaStatusDto | null>(null);
  protected readonly selectedModel = signal("");

  protected readonly modelOptions = computed(() => {
    const tier = this.quotaStatus()?.allowedModelTier?.toLowerCase() ?? "base";
    return allModels.filter((m) => {
      if (m.tier === "base") return true;
      if (m.tier === "premium") return tier === "premium" || tier === "ultra";
      if (m.tier === "ultra") return tier === "ultra";
      return false;
    });
  });

  protected readonly selectedModelInfo = computed(() => {
    const value = this.selectedModel();
    return allModels.find((m) => m.value === value) ?? allModels[0];
  });

  protected readonly showMultiplierWarning = computed(() => {
    return this.selectedModelInfo().multiplier > 1;
  });

  ngOnInit(): void {
    this.loadQuotaStatus();
  }

  private async loadQuotaStatus(): Promise<void> {
    this.isLoading.set(true);
    try {
      const quota = await this.api.invoke(getAiQuotaStatus);
      this.quotaStatus.set(quota);
      this.selectedModel.set(quota.currentModel ?? "");
    } catch {
      this.toastService.showError("Failed to load AI settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected async save(): Promise<void> {
    this.isSaving.set(true);
    try {
      await this.api.invoke(updateTenantAiSettings, {
        body: {
          model: this.selectedModel() || undefined,
        },
      });
      this.toastService.showSuccess("AI settings saved successfully");
    } catch {
      this.toastService.showError("Failed to save AI settings");
    } finally {
      this.isSaving.set(false);
    }
  }
}
