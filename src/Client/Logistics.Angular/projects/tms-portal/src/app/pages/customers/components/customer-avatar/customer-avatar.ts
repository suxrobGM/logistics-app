import { Component, computed, input } from "@angular/core";
import { type CustomerDto } from "@logistics/shared/api";
import { type AvatarColors, type AvatarSize, UserAvatar } from "@/shared/components";
import { Converters } from "@/shared/utils";

@Component({
  selector: "app-customer-avatar",
  templateUrl: "./customer-avatar.html",
  imports: [UserAvatar],
})
export class CustomerAvatar {
  public readonly customer = input.required<CustomerDto | null>();
  public readonly size = input<AvatarSize>("normal");

  protected readonly initials = computed(() => Converters.getInitials(this.customer()?.name));

  protected readonly colors = computed<AvatarColors>(() => {
    const status = this.customer()?.status;
    switch (status) {
      case "active":
        return {
          bg: "color-mix(in srgb, var(--success) 15%, transparent)",
          text: "var(--success)",
        };
      case "prospect":
        return { bg: "color-mix(in srgb, var(--info) 15%, transparent)", text: "var(--info)" };
      case "inactive":
        return { bg: "var(--bg-hover)", text: "var(--text-secondary)" };
      default:
        return {
          bg: "color-mix(in srgb, var(--primary-500) 15%, transparent)",
          text: "var(--primary-500)",
        };
    }
  });
}
