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
 * What `/onboarding/progress` can emit. Any key may be absent: the server drops steps whose tenant
 * feature is off, and `inviteTeam` in solo-operator mode.
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
 * Exhaustive on purpose: a step added to the union without copy here is a compile error rather than
 * a silently missing row.
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
      const key = step.key as OnboardingStepKey | undefined;
      if (!key) return [];

      const meta: OnboardingStepMeta | undefined = ONBOARDING_STEPS[key];
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
   * False while the fetch is in flight on purpose: this sits at the top of the dashboard, and a
   * placeholder that vanishes for every established tenant costs more than it buys the new one.
   */
  protected readonly isVisible = computed(
    () => !this.dismissed() && this.totalCount() > 0 && this.completedCount() < this.totalCount(),
  );

  ngOnInit(): void {
    // The whole template hangs off `isVisible`, so a dismissed checklist has nothing to render and
    // the endpoint is uncached by design - skipping the fetch saves it on every dashboard visit.
    if (this.dismissed()) return;
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
