import { Injectable, inject, signal } from "@angular/core";
import type { IUpgradePromptHandler } from "@logistics/shared";
import { Api, getSubscriptionPlans } from "@logistics/shared/api";
import type { PlanTier, SubscriptionPlanDto } from "@logistics/shared/api";
import { TenantService } from "./tenant.service";

export type UpgradeReason = "feature_not_in_plan" | "resource_limit";

export interface UpgradePromptData {
  reason: UpgradeReason;
  message: string;
  currentPlan?: SubscriptionPlanDto;
  recommendedPlan?: SubscriptionPlanDto;
}

/** Plan tier ordering for determining the next upgrade tier. */
const TIER_ORDER: PlanTier[] = ["starter", "professional", "enterprise"];

/**
 * Service for showing upgrade prompts when users hit feature or resource limits.
 * Implements the IUpgradePromptHandler interface so it can be used by the feature guard.
 * The service fetches subscription plans to recommend the next best plan to the user.
 */
@Injectable({ providedIn: "root" })
export class UpgradePromptService implements IUpgradePromptHandler {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantService);

  public readonly visible = signal(false);
  public readonly data = signal<UpgradePromptData | null>(null);

  private plansCache: SubscriptionPlanDto[] | null = null;

  showUpgradePrompt(errorCode: string, message: string): void {
    const reason: UpgradeReason =
      errorCode === "RESOURCE_LIMIT_REACHED" ? "resource_limit" : "feature_not_in_plan";

    const currentPlan = this.tenantService.getTenantData()?.subscription?.plan;

    this.data.set({
      reason,
      message,
      currentPlan: currentPlan ?? undefined,
    });
    this.visible.set(true);

    // Fetch plans in background to populate recommended plan
    this.loadPlansAndRecommend(currentPlan?.tier);
  }

  hide(): void {
    this.visible.set(false);
    this.data.set(null);
  }

  private async loadPlansAndRecommend(currentTier?: PlanTier): Promise<void> {
    const plans = await this.getPlans();
    if (!plans.length) return;

    const recommendedPlan = this.getRecommendedPlan(plans, currentTier);

    this.data.update((current) => {
      if (!current) return current;
      return {
        ...current,
        currentPlan: current.currentPlan ?? plans.find((p) => p.tier === currentTier),
        recommendedPlan: recommendedPlan ?? undefined,
      };
    });
  }

  private async getPlans(): Promise<SubscriptionPlanDto[]> {
    if (this.plansCache) {
      return this.plansCache;
    }

    try {
      const result = await this.api.invoke(getSubscriptionPlans, {
        Page: 1,
        PageSize: 10,
      });

      this.plansCache = result?.items ?? [];
      return this.plansCache;
    } catch {
      console.error("Failed to fetch subscription plans");
      return [];
    }
  }

  private getRecommendedPlan(
    plans: SubscriptionPlanDto[],
    currentTier?: PlanTier,
  ): SubscriptionPlanDto | null {
    if (!currentTier) {
      // No current plan — recommend the lowest tier
      return plans.find((p) => p.tier === "starter") ?? null;
    }

    const currentIndex = TIER_ORDER.indexOf(currentTier);
    const nextTier = TIER_ORDER[currentIndex + 1];

    if (!nextTier) return null; // Already on highest tier

    return plans.find((p) => p.tier === nextTier) ?? null;
  }
}
