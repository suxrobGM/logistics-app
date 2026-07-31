import { Component, inject, signal, type OnInit } from "@angular/core";
import {
  Api,
  getAISettings,
  updateAISettings,
  type PlanQuotaDto,
  type ReasoningEffort,
} from "@logistics/shared/api";
import { reasoningEffortOptions } from "@logistics/shared/api/enums";
import type { SelectOption } from "@logistics/shared/models";
import {
  Alert,
  Card,
  CurrencyField,
  Grid,
  PageHeader,
  Spinner,
  Stack,
  Typography,
  UiButton,
  UiSelectField,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { TenantQuotas } from "../tenant-quotas/tenant-quotas";

@Component({
  selector: "adm-ai-settings",
  templateUrl: "./ai-settings.html",
  imports: [
    Alert,
    Card,
    CurrencyField,
    Grid,
    PageHeader,
    Spinner,
    Stack,
    TenantQuotas,
    Typography,
    UiButton,
    UiSelectField,
  ],
})
export class AISettings implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(true);
  protected readonly isSaving = signal(false);

  protected readonly selectedModel = signal("");
  protected readonly reasoningEffort = signal<ReasoningEffort>("none");
  protected readonly modelOptions = signal<SelectOption<string>[]>([]);
  protected readonly plans = signal<PlanQuotaDto[]>([]);

  protected readonly effortOptions = reasoningEffortOptions;

  ngOnInit(): void {
    this.load();
  }

  private async load(): Promise<void> {
    this.isLoading.set(true);
    try {
      const settings = await this.api.invoke(getAISettings);
      this.selectedModel.set(settings.model ?? "");
      this.reasoningEffort.set(settings.reasoningEffort ?? "none");
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

  /** The control clears to null; the DTO field is optional - hence the one conversion. */
  protected updatePlanBudget(planId: string | undefined, budgetUsd: number | null): void {
    this.plans.update((plans) =>
      plans.map((p) =>
        p.planId === planId ? { ...p, weeklyAIBudgetUsd: budgetUsd ?? undefined } : p,
      ),
    );
  }

  protected async save(): Promise<void> {
    if (this.plans().some((p) => !p.weeklyAIBudgetUsd)) {
      this.toastService.showError("Every plan needs a weekly AI budget greater than zero");
      return;
    }

    this.isSaving.set(true);
    try {
      await this.api.invoke(updateAISettings, {
        body: {
          model: this.selectedModel(),
          reasoningEffort: this.reasoningEffort(),
          plans: this.plans().map((p) => ({
            planId: p.planId,
            weeklyAIBudgetUsd: p.weeklyAIBudgetUsd,
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
