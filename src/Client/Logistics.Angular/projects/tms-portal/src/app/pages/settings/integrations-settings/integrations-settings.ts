import { Component, computed, inject } from "@angular/core";
import { Permission } from "@logistics/shared";
import { FeatureService } from "@logistics/shared/services";
import { Container, Stack } from "@logistics/shared/ui";
import { PermissionService } from "@/core/auth";
import { EmptyState, PageHeader } from "@/shared/components";
import { ApiKeysTable, McpIntegrationGuide, QuickbooksCard } from "../components";

/**
 * External systems the tenant can connect. The route admits either feature flag, so each card gates
 * itself: QuickBooks on `accounting`, API keys on `mcp_server` plus `ApiKey.View` (Owner only).
 */
@Component({
  selector: "app-integrations-settings",
  templateUrl: "./integrations-settings.html",
  imports: [
    ApiKeysTable,
    Container,
    EmptyState,
    McpIntegrationGuide,
    PageHeader,
    QuickbooksCard,
    Stack,
  ],
})
export class IntegrationsSettings {
  private readonly featureService = inject(FeatureService);
  private readonly permissionService = inject(PermissionService);

  protected readonly showAccounting = computed(() => this.featureService.isEnabled("accounting"));

  protected readonly showApiKeys = computed(
    () =>
      this.featureService.isEnabled("mcp_server") &&
      this.permissionService.hasPermission(Permission.ApiKey.View),
  );
}
