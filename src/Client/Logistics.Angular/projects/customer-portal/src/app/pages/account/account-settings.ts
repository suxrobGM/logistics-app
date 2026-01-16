import { DatePipe } from "@angular/common";
import { Component, inject, signal } from "@angular/core";
import { Api, type CustomerUserDto, getPortalProfile } from "@logistics/shared/api";
import { ButtonModule } from "primeng/button";
import { CardModule } from "primeng/card";
import { ProgressSpinnerModule } from "primeng/progressspinner";
import { TenantContextService } from "@/core/services";
import { environment } from "@/env";

@Component({
  selector: "cp-account-settings",
  templateUrl: "./account-settings.html",
  imports: [DatePipe, CardModule, ButtonModule, ProgressSpinnerModule],
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
