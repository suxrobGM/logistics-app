import { Component, computed, inject, signal, type OnInit } from "@angular/core";
import {
  Api,
  createConnectAccount,
  getDashboardLink,
  getOnboardingLink,
  refreshConnectStatus,
  type StripeConnectStatus,
  type StripeConnectStatusDto,
} from "@logistics/shared/api";
import {
  Badge,
  Card,
  Grid,
  Icon,
  Stack,
  Surface,
  Typography,
  UiButton,
  type UiBadgeIntent,
} from "@logistics/shared/ui";
import { ToastService } from "@/core/services";
import { EmptyState, LoadingSkeleton } from "@/shared/components";

/**
 * Exhaustive on purpose: `StripeConnectStatus` is generated from the API spec, so a status added
 * upstream becomes a compile error here instead of silently rendering as "Not Connected".
 */
const STATUS_META: Record<StripeConnectStatus, { severity: UiBadgeIntent; label: string }> = {
  not_connected: { severity: "secondary", label: "Not Connected" },
  pending: { severity: "warn", label: "Pending Verification" },
  active: { severity: "success", label: "Active" },
  restricted: { severity: "warn", label: "Restricted" },
  disabled: { severity: "danger", label: "Disabled" },
};

/** Stripe Connect onboarding + payout status. Rendered by the Billing settings tab. */
@Component({
  selector: "app-stripe-connect-card",
  templateUrl: "./stripe-connect-card.html",
  imports: [
    Badge,
    Card,
    EmptyState,
    Grid,
    Icon,
    LoadingSkeleton,
    Stack,
    Surface,
    Typography,
    UiButton,
  ],
})
export class StripeConnectCard implements OnInit {
  private readonly api = inject(Api);
  private readonly toastService = inject(ToastService);

  protected readonly isLoading = signal(false);
  protected readonly isCreatingAccount = signal(false);
  protected readonly isGettingLink = signal(false);
  protected readonly isOpeningDashboard = signal(false);
  protected readonly connectStatus = signal<StripeConnectStatusDto | null>(null);

  private readonly statusMeta = computed(
    () => STATUS_META[this.connectStatus()?.status ?? "not_connected"],
  );

  protected readonly statusSeverity = computed(() => this.statusMeta().severity);
  protected readonly statusLabel = computed(() => this.statusMeta().label);

  ngOnInit(): void {
    this.fetchConnectStatus();
  }

  protected async startOnboarding(): Promise<void> {
    const status = this.connectStatus();

    // If no account exists, create one first
    if (!status?.accountId) {
      await this.createAccount();
    }

    // Then get the onboarding link
    await this.redirectToOnboarding();
  }

  protected async continueOnboarding(): Promise<void> {
    await this.redirectToOnboarding();
  }

  protected async openDashboard(): Promise<void> {
    this.isOpeningDashboard.set(true);
    try {
      const result = await this.api.invoke(getDashboardLink, {});
      window.open(result.url!, "_blank");
    } catch {
      this.toastService.showError("Failed to open Stripe dashboard");
    } finally {
      this.isOpeningDashboard.set(false);
    }
  }

  private async createAccount(): Promise<void> {
    this.isCreatingAccount.set(true);
    try {
      await this.api.invoke(createConnectAccount, {});
      await this.fetchConnectStatus();
    } catch {
      this.toastService.showError("Failed to create Stripe Connect account");
    } finally {
      this.isCreatingAccount.set(false);
    }
  }

  private async redirectToOnboarding(): Promise<void> {
    this.isGettingLink.set(true);
    try {
      const currentUrl = window.location.href;
      const result = await this.api.invoke(getOnboardingLink, {
        returnUrl: currentUrl,
        refreshUrl: currentUrl,
      });

      window.location.href = result.url!;
    } catch {
      this.toastService.showError("Failed to get onboarding link");
    } finally {
      this.isGettingLink.set(false);
    }
  }

  private async fetchConnectStatus(): Promise<void> {
    this.isLoading.set(true);
    try {
      const status = await this.api.invoke(refreshConnectStatus, {});
      this.connectStatus.set(status);
    } catch {
      this.toastService.showError("Failed to load payment settings");
    } finally {
      this.isLoading.set(false);
    }
  }
}
