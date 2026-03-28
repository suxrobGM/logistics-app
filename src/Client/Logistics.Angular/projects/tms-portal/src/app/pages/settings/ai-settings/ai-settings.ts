import { Component, type OnInit, computed, inject, signal } from "@angular/core";
import { FormsModule } from "@angular/forms";
import {
  Api,
  type AiQuotaStatusDto,
  type LlmProvider,
  getAiQuotaStatus,
  updateTenantAiSettings,
} from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
import { TagModule } from "primeng/tag";
import { ToastService } from "@/core/services";
import { LabeledField } from "@/shared/components";

interface SelectOption {
  label: string;
  value: string;
}

const providerOptions: SelectOption[] = [
  { label: "System Default", value: "" },
  { label: "Anthropic", value: "anthropic" },
  { label: "OpenAI", value: "open_ai" },
  { label: "DeepSeek", value: "deep_seek" },
];

const baseModels: SelectOption[] = [
  { label: "DeepSeek Chat", value: "deepseek-chat" },
  { label: "DeepSeek Reasoner", value: "deepseek-reasoner" },
  { label: "GPT-5.4 Mini", value: "gpt-5.4-mini" },
  { label: "Claude Haiku 4.5", value: "claude-haiku-4-5" },
];

const premiumModels: SelectOption[] = [
  { label: "GPT-5.4", value: "gpt-5.4" },
  { label: "Claude Sonnet 4.6", value: "claude-sonnet-4-6" },
];

const ultraModels: SelectOption[] = [
  { label: "Claude Opus 4.6", value: "claude-opus-4-6" },
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
    LabeledField,
  ],
})
export class AiSettingsComponent implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);
  protected readonly quotaStatus = signal<AiQuotaStatusDto | null>(null);
  protected readonly selectedProvider = signal("");
  protected readonly selectedModel = signal("");

  protected readonly providerOptions = providerOptions;

  /** Available models filtered by the tenant's allowed model tier */
  protected readonly modelOptions = computed<SelectOption[]>(() => {
    const tier = this.quotaStatus()?.allowedModelTier?.toLowerCase() ?? "base";
    const systemDefault: SelectOption = { label: "System Default", value: "" };

    let models = [...baseModels];
    if (tier === "premium" || tier === "ultra") {
      models = [...models, ...premiumModels];
    }
    if (tier === "ultra") {
      models = [...models, ...ultraModels];
    }

    return [systemDefault, ...models];
  });

  ngOnInit(): void {
    this.loadQuotaStatus();
  }

  private async loadQuotaStatus(): Promise<void> {
    this.isLoading.set(true);
    try {
      const quota = await this.api.invoke(getAiQuotaStatus);
      this.quotaStatus.set(quota);
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
          provider: (this.selectedProvider() || undefined) as LlmProvider | undefined,
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
