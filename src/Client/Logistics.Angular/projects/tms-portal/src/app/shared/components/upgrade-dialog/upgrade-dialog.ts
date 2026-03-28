import { CurrencyPipe } from "@angular/common";
import { Component, computed, inject } from "@angular/core";
import { Router } from "@angular/router";
import type { SubscriptionPlanDto } from "@logistics/shared/api";
import { FEATURE_DESCRIPTIONS } from "@logistics/shared/services";
import { ButtonModule } from "primeng/button";
import { DialogModule } from "primeng/dialog";
import { TagModule } from "primeng/tag";
import { UpgradePromptService } from "@/core/services";
import { Labels } from "@/shared/utils";

@Component({
  selector: "app-upgrade-dialog",
  templateUrl: "./upgrade-dialog.html",
  imports: [DialogModule, ButtonModule, TagModule, CurrencyPipe],
})
export class UpgradeDialog {
  private readonly router = inject(Router);
  protected readonly upgradeService = inject(UpgradePromptService);

  protected readonly data = this.upgradeService.data;
  protected readonly visible = this.upgradeService.visible;

  protected readonly isResourceLimit = computed(() => this.data()?.reason === "resource_limit");
  protected readonly currentPlanLabel = computed(() => this.data()?.currentPlan?.name ?? "Free");
  protected readonly recommendedPlanLabel = computed(
    () => this.data()?.recommendedPlan?.name ?? "a higher plan",
  );

  protected getMaxTrucksLabel(plan: SubscriptionPlanDto): string {
    return plan.maxTrucks ? `Up to ${plan.maxTrucks} trucks` : "Unlimited trucks";
  }

  protected getAiQuotaLabel(plan: SubscriptionPlanDto): string {
    switch (plan.allowedModelTier) {
      case "ultra":
        return "All AI models incl. Opus, 8× usage";
      case "premium":
        return "Base + premium AI models, 4× usage";
      default:
        return "Base AI models";
    }
  }

  protected getTierSeverity(plan: SubscriptionPlanDto): "info" | "warn" | "success" {
    switch (plan.tier) {
      case "starter":
        return "info";
      case "professional":
        return "warn";
      case "enterprise":
        return "success";
      default:
        return "info";
    }
  }

  protected getExtraFeatures(
    current: SubscriptionPlanDto | null | undefined,
    recommended: SubscriptionPlanDto | null | undefined,
  ): string[] {
    if (!recommended?.features) {
      return [];
    }
    const currentFeatures = new Set<string>(current?.features ?? []);

    return recommended.features
      .filter((f) => !currentFeatures.has(f))
      .map((f) => FEATURE_DESCRIPTIONS[f] ?? f);
  }

  protected tierLabel(plan: SubscriptionPlanDto): string {
    return Labels.planTierLabel(plan.tier!);
  }

  protected viewPlans(): void {
    this.upgradeService.hide();
    this.router.navigate(["/subscription/plans"]);
  }

  protected upgradeNow(): void {
    const tier = this.data()?.recommendedPlan?.tier;
    this.upgradeService.hide();
    this.router.navigate(["/subscription/plans"], tier ? { queryParams: { upgrade: tier } } : {});
  }

  protected dismiss(): void {
    this.upgradeService.hide();
  }
}
