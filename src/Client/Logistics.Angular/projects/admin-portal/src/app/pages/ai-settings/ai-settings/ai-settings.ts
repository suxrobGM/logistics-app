import { Component, inject, signal, type OnInit } from "@angular/core";
import { Api, getAiSettings, updateAiSettings, type PlanQuotaDto } from "@logistics/shared/api";
import {
  Alert,
  Card,
  Grid,
  PageHeader,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiCheckboxField,
  UiNumberField,
  UiSelectField,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { TenantQuotas } from "../tenant-quotas/tenant-quotas";

interface ModelOption {
  label: string;
  value: string;
}

@Component({
  selector: "adm-ai-settings",
  templateUrl: "./ai-settings.html",
  imports: [
    Alert,
    Card,
    Grid,
    PageHeader,
    Spinner,
    Stack,
    TenantQuotas,
    Typography,
    UiButton,
    UiCheckboxField,
    UiNumberField,
    UiSelectField,
  ],
})
export class AiSettings implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);

  protected readonly selectedModel = signal("");
  protected readonly extendedThinking = signal(false);
  protected readonly modelOptions = signal<ModelOption[]>([]);
  protected readonly plans = signal<PlanQuotaDto[]>([]);

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const settings = await this.api.invoke(getAiSettings);
      this.selectedModel.set(settings.model ?? "");
      this.extendedThinking.set(settings.extendedThinking ?? false);
      this.modelOptions.set(
        (settings.availableModels ?? []).map((m) => ({
          label: m.displayName ?? m.id ?? "",
          value: m.id ?? "",
        })),
      );
      this.plans.set(settings.plans ?? []);
    } catch {
      this.toastService.showError("Failed to load AI settings");
    } finally {
      this.isLoading.set(false);
    }
  }

  protected updatePlanQuota(planId: string | undefined, quota: number | null): void {
    this.plans.update((plans) =>
      plans.map((p) => (p.planId === planId ? { ...p, weeklyAiRequestQuota: quota } : p)),
    );
  }

  protected async save(): Promise<void> {
    this.isSaving.set(true);
    try {
      await this.api.invoke(updateAiSettings, {
        body: {
          model: this.selectedModel(),
          extendedThinking: this.extendedThinking(),
          plans: this.plans().map((p) => ({
            planId: p.planId,
            weeklyAiRequestQuota: p.weeklyAiRequestQuota,
          })),
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
