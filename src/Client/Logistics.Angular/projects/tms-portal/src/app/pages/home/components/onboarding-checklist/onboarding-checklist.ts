import { isPlatformBrowser } from "@angular/common";
import { Component, computed, inject, PLATFORM_ID, signal, type OnInit } from "@angular/core";
import { RouterLink } from "@angular/router";
import { Api, getOnboardingProgress, type OnboardingStepDto } from "@logistics/shared/api";
import {
  Card,
  Divider,
  Icon,
  Progress,
  Stack,
  Typography,
  UiButton,
  type IconName,
} from "@logistics/shared/ui";

const DISMISSED_KEY = "tms-onboarding-dismissed";

/**
 * The keys `/onboarding/progress` emits, in the order it emits them. `inviteTeam` is omitted from
 * the response entirely in solo-operator mode.
 */
type OnboardingStepKey =
  | "companyProfile"
  | "addTruck"
  | "inviteTeam"
  | "addCustomer"
  | "firstLoad"
  | "getPaid"
  | "connectEld";

interface OnboardingStepMeta {
  label: string;
  cta: string;
  icon: IconName;
  route: string;
}

/**
 * Copy and destinations live on the client on purpose: changing a label must not need a backend
 * deploy. Exhaustive over `OnboardingStepKey`, so a step added to that union without copy here is a
 * compile error rather than a silently missing row.
 */
const ONBOARDING_STEPS: Record<OnboardingStepKey, OnboardingStepMeta> = {
  companyProfile: {
    label: "Fill in your company details",
    cta: "Finish profile",
    icon: "building-2",
    route: "/settings/company",
  },
  addTruck: {
    label: "Add your first truck",
    cta: "Add truck",
    icon: "truck",
    route: "/trucks/add",
  },
  inviteTeam: {
    label: "Invite a dispatcher or driver",
    cta: "Invite",
    icon: "user-plus",
    route: "/employees",
  },
  addCustomer: {
    label: "Add a broker or shipper you haul for",
    cta: "Add customer",
    icon: "briefcase",
    route: "/customers/add",
  },
  firstLoad: {
    label: "Book your first load",
    cta: "Add load",
    icon: "box",
    route: "/loads/add",
  },
  getPaid: {
    label: "Set up payouts so you get paid",
    cta: "Set up payouts",
    icon: "credit-card",
    route: "/settings/billing",
  },
  connectEld: {
    label: "Connect your ELD for hours and location",
    cta: "Connect ELD",
    icon: "gauge",
    route: "/eld/providers",
  },
};

interface OnboardingRow extends OnboardingStepMeta {
  key: string;
  isComplete: boolean;
}

/**
 * Setup checklist for a new tenant. Fetches its own progress, so it takes no inputs and can be
 * dropped anywhere on the dashboard. Never blocks: it renders nothing once every step is done, on a
 * failed fetch, or after the owner dismisses it.
 */
@Component({
  selector: "app-onboarding-checklist",
  templateUrl: "./onboarding-checklist.html",
  imports: [Card, Divider, Icon, Progress, RouterLink, Stack, Typography, UiButton],
})
export class OnboardingChecklist implements OnInit {
  private readonly api = inject(Api);
  private readonly platformId = inject(PLATFORM_ID);

  private readonly steps = signal<OnboardingStepDto[]>([]);
  protected readonly dismissed = signal(this.readDismissed());

  protected readonly rows = computed<OnboardingRow[]>(() =>
    this.steps().flatMap((step) => {
      const key = step.key;
      if (!key) {
        return [];
      }

      const meta = Object.hasOwn(ONBOARDING_STEPS, key)
        ? ONBOARDING_STEPS[key as OnboardingStepKey]
        : undefined;

      return meta ? [{ ...meta, key, isComplete: step.isComplete === true }] : [];
    }),
  );

  protected readonly totalCount = computed(() => this.rows().length);
  protected readonly completedCount = computed(
    () => this.rows().filter((row) => row.isComplete).length,
  );

  protected readonly percentComplete = computed(() => {
    const total = this.totalCount();
    return total === 0 ? 0 : (this.completedCount() / total) * 100;
  });

  /**
   * Deliberately false while the fetch is in flight: this sits at the top of the dashboard, so a
   * placeholder that vanishes for every established tenant costs more than it buys the new one.
   */
  protected readonly isVisible = computed(
    () => !this.dismissed() && this.totalCount() > 0 && this.completedCount() < this.totalCount(),
  );

  ngOnInit(): void {
    void this.fetchProgress();
  }

  protected dismiss(): void {
    this.dismissed.set(true);

    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(DISMISSED_KEY, "true");
    }
  }

  private async fetchProgress(): Promise<void> {
    try {
      const progress = await this.api.invoke(getOnboardingProgress, {});
      this.steps.set(progress.steps ?? []);
    } catch {
      this.steps.set([]);
    }
  }

  private readDismissed(): boolean {
    return isPlatformBrowser(this.platformId) && localStorage.getItem(DISMISSED_KEY) === "true";
  }
}
