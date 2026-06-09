import { Component, inject, signal, type OnInit } from "@angular/core";
import { FormsModule } from "@angular/forms";
import { Api, getAiSettings, updateAiSettings, type PlanQuotaDto } from "@logistics/shared/api";
import { Grid, PageHeader, Stack, Typography } from "@logistics/shared/components";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { CheckboxModule } from "primeng/checkbox";
import { InputNumberModule } from "primeng/inputnumber";
import { MessageModule } from "primeng/message";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { SelectModule } from "primeng/select";
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
    FormsModule,
    ButtonModule,
    CardModule,
    CheckboxModule,
    InputNumberModule,
    MessageModule,
    ProgressSpinnerModule,
    SelectModule,
    Grid,
    PageHeader,
    Stack,
    Typography,
    TenantQuotas,
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
