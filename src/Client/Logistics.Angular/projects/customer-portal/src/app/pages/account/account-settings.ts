import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Api, getPortalProfile, type CustomerUserDto } from "@logistics/shared/api";
import { Grid, Spinner, Stack, Surface, Typography, UiButton } from "@logistics/shared/ui";
import { TenantContextService } from "@/core/services";
import { environment } from "@/env";

@Component({
  selector: "cp-account-settings",
  templateUrl: "./account-settings.html",
  imports: [DatePipe, Grid, Spinner, Stack, Surface, Typography, UiButton],
})
export class AccountSettings {
  private readonly api = inject(Api);
  private readonly tenantService = inject(TenantContextService);

  protected readonly profile = signal<CustomerUserDto | null>(null);
  protected readonly isLoading = signal(true);
  protected readonly companyName = signal<string | null>(null);

  constructor() {
    this.loadProfile();
  }

  private async loadProfile(): Promise<void> {
    try {
      const tenant = this.tenantService.currentTenant();
      this.companyName.set(tenant?.companyName ?? tenant?.tenantName ?? null);

      const profile = await this.api.invoke(getPortalProfile);
      this.profile.set(profile);
    } catch (error) {
      console.error("Failed to load profile:", error);
    } finally {
      this.isLoading.set(false);
    }
  }

  protected openManageProfile(): void {
    window.open(`${environment.identityServerUrl}/account/manage/profile`, "_blank");
  }

  protected openChangePassword(): void {
    window.open(`${environment.identityServerUrl}/account/manage/changepassword`, "_blank");
  }
}
