import { Component, computed, input } from "@angular/core";
import type { EldProviderType } from "@logistics/shared/api";
import { Icon, UiTooltip, type IconName } from "@logistics/shared/ui";

interface ProviderInfo {
  label: string;
  icon: IconName;
  color: string;
}

const PROVIDER_INFO: Record<EldProviderType, ProviderInfo> = {
  samsara: { label: "Samsara", icon: "wifi", color: "text-blue-600" },
  motive: { label: "Motive", icon: "wifi", color: "text-green-600" },
  geotab: { label: "Geotab", icon: "wifi", color: "text-purple-600" },
  omnitracs: { label: "Omnitracs", icon: "wifi", color: "text-orange-600" },
  people_net: { label: "PeopleNet", icon: "wifi", color: "text-teal-600" },
  tt_eld: { label: "T&T ELD", icon: "wifi", color: "text-red-600" },
  demo: { label: "Demo", icon: "wifi", color: "text-gray-600" },
};

@Component({
  selector: "app-eld-status-badge",
  templateUrl: "./eld-status-badge.html",
  imports: [Icon, UiTooltip],
})
export class EldStatusBadge {
  public readonly providerType = input<EldProviderType | null>(null);
  public readonly isConnected = input(false);
  public readonly lastSyncAt = input<string | null>(null);

  protected readonly providerInfo = computed(() => {
    const type = this.providerType();
    if (!type) return null;
    return PROVIDER_INFO[type] ?? { label: type, icon: "wifi", color: "text-gray-600" };
  });

  protected readonly statusText = computed(() => {
    const provider = this.providerInfo();
    if (!provider) return "No ELD";

    if (this.isConnected()) {
      return `${provider.label} connected`;
    }
    return `${provider.label} disconnected`;
  });

  protected readonly lastSyncText = computed(() => {
    const lastSync = this.lastSyncAt();
    if (!lastSync) return null;

    const date = new Date(lastSync);
    const now = new Date();
    const diffMinutes = Math.floor((now.getTime() - date.getTime()) / 60000);

    if (diffMinutes < 1) return "Just now";
    if (diffMinutes < 60) return `${diffMinutes}m ago`;
    if (diffMinutes < 1440) return `${Math.floor(diffMinutes / 60)}h ago`;
    return `${Math.floor(diffMinutes / 1440)}d ago`;
  });
}
